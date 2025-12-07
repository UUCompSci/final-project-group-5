

// ===== OBJECT CREATION =====

// Need to create cell objects that are not the player's cell and give them a size and name.
// Need to create Clusters that contain many cells
// DO NOT need to create food objects because food is an enum of just "meat" and "plant"

// ===== RUNTIME =====

// Cells can be created through user input. ie. "Create Carnivore: 'Bobby'"
// Player's cell is size 3 by default.
// Cells encounter plant and meat food and can choose to eat it
// Cells can encounter other cells and eat them if carnivores
// Other Cell's size is then added to the Cell's size
// Agar.io
// Cells can encounter Clusters of other cells which they can join the cluster (Eventually that will add them to the cluster in the database)
// This acts like a "House"
using System;
using System.Collections.Generic;
using System.Linq;
using Entities;


namespace CellWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            List<LivingThing> allCells = new List<LivingThing>();
            List<Cluster> clusters = new List<Cluster>();
            LivingThing? player = null;

            bool running = true;
            ShowMenu();

            while (running)
            {
                Console.Write("\nPress a key: ");
                ConsoleKey key = Console.ReadKey(true).Key;

                try
                {
                    switch (key)
                    {
                        case ConsoleKey.C:
                            CreateCell(allCells, ref player);
                            break;

                        case ConsoleKey.E:
                            if (player == null)
                            {
                                Console.WriteLine("You must create a player cell first.");
                                break;
                            }
                            if (CheckEvolution(player, clusters))
                                HandleEvolution(ref player);
                            else
                                HandleEncounter(player, allCells, clusters);
                            break;

                        case ConsoleKey.M:
                            MakeCluster(clusters);
                            break;

                        case ConsoleKey.S:
                            if (player == null)
                            {
                                Console.WriteLine("No player cell created yet.");
                                break;
                            }
                            ShowStats(player, clusters);
                            break;

                        case ConsoleKey.Q:
                            running = false;
                            Console.WriteLine("Exiting simulation...");
                            break;

                        default:
                            Console.WriteLine("Invalid key.");
                            break;
                    }

                    if (running)
                        ShowMenu();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    if (running)
                        ShowMenu();
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("\nAvailable Keys:");
            Console.WriteLine(" [C] Create Cell");
            Console.WriteLine(" [E] Encounter / Evolve");
            Console.WriteLine(" [M] Make Cluster");
            Console.WriteLine(" [S] Stats");
            Console.WriteLine(" [Q] Quit");
        }

        static void CreateCell(List<LivingThing> allCells, ref LivingThing? player)
        {
            Console.Write("\nEnter type (Carnivore/Herbivore): ");
            string? typeInput = Console.ReadLine()?.Trim().ToLower();
            if (typeInput != "carnivore" && typeInput != "herbivore")
            {
                Console.WriteLine("Invalid type.");
                return;
            }

            Console.Write("Enter name: ");
            string? name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Invalid name.");
                return;
            }

            LivingThing newCell = typeInput switch
            {
                "carnivore" => new Carnivore(name),
                "herbivore" => new Herbivore(name),
                _ => throw new InvalidOperationException("Unknown type.")
            };

            allCells.Add(newCell);

            if (player == null)
            {
                player = newCell;
                player.Size = 3;
                Console.WriteLine($"Created {typeInput} named {player.Name} (Size: {player.Size})");
            }
            else
            {
                Console.WriteLine($"Created {typeInput} named {newCell.Name} (Size: {newCell.Size})");
            }
        }

        static void MakeCluster(List<Cluster> clusters)
        {
            Console.Write("\nEnter cluster name: ");
            string? clusterName = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(clusterName))
            {
                Console.WriteLine("Invalid cluster name.");
                return;
            }

            Cluster cluster = new Cluster(clusterName);
            clusters.Add(cluster);
            Console.WriteLine($"Created Cluster '{cluster.Name}'.");
        }

        static void ShowStats(LivingThing player, List<Cluster> clusters)
        {
            Console.WriteLine($"\n{player.Name} - Size: {player.Size}, Type: {player.GetType().Name}");

            if (player is Species s)
                Console.WriteLine($"Stats -> Health: {s.Health}, Speed: {s.Speed}, Attack: {s.Attack}");

            var playerClusters = clusters.Where(c => c.Cells.Contains(player)).ToList();

            if (playerClusters.Count == 0)
                Console.WriteLine("Clusters: None");
            else
                Console.WriteLine("Clusters: " + string.Join(", ", playerClusters.Select(c => c.Name)));
        }

        static void HandleEncounter(LivingThing player, List<LivingThing> allCells, List<Cluster> clusters)
        {
            Random rand = new Random();
            int eventType = rand.Next(3);

            switch (eventType)
            {
                case 0:
                    EncounterFood(player, rand);
                    break;

                case 1:
                    EncounterCell(player, allCells, rand);
                    break;

                case 2:
                    EncounterCluster(player, clusters, rand);
                    break;
            }
        }

        static void EncounterFood(LivingThing player, Random rand)
        {
            FoodType food = rand.Next(2) == 0 ? FoodType.Plant : FoodType.Meat;
            Console.WriteLine($"\nYou encountered {food}.");

            Console.Write("Eat it? (Y/N): ");
            ConsoleKey response = Console.ReadKey(true).Key;

            if (response == ConsoleKey.Y)
            {
                try
                {
                    player.Eat(food);
                    Console.WriteLine($"{player.Name} ate the {food}. Size is now {player.Size}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"{player.Name} ignored the {food}.");
            }
        }

        static void EncounterCell(LivingThing player, List<LivingThing> allCells, Random rand)
        {
            if (rand.NextDouble() < 0.5 || allCells.Count == 0)
            {
                LivingThing wild = GenerateRandomCell(rand);
                allCells.Add(wild);
            }

            LivingThing target = allCells[rand.Next(allCells.Count)];
            if (target == player)
            {
                Console.WriteLine("You encountered your reflection.");
                return;
            }

            Console.WriteLine($"\nYou encountered another cell: {target.Name} (Type: {target.GetType().Name}, Size: {target.Size})");

            if (player is Carnivore carnivorePlayer)
            {
                if (player.Size <= target.Size)
                {
                    Console.WriteLine($"{player.Name} is too small to eat {target.Name}.");
                    return;
                }

                Console.Write("Eat it? (Y/N): ");
                ConsoleKey response = Console.ReadKey(true).Key;

                if (response == ConsoleKey.Y)
                {
                    try
                    {
                        carnivorePlayer.Eat(target);
                        allCells.Remove(target);
                        Console.WriteLine($"{player.Name} is now size {player.Size}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"{player.Name} chose not to eat {target.Name}.");
                }
            }
            else if (player is Omnivore omni)
            {
                if (player.Size <= target.Size)
                {
                    Console.WriteLine($"{player.Name} is too small to eat {target.Name}.");
                    return;
                }

                Console.Write("Eat it? (Y/N): ");
                ConsoleKey response = Console.ReadKey(true).Key;

                if (response == ConsoleKey.Y)
                {
                    omni.Eat(target);
                    allCells.Remove(target);
                    Console.WriteLine($"{player.Name} is now size {player.Size}.");
                }
            }
            else
            {
                Console.WriteLine($"{player.Name} cannot eat other creatures.");
            }
        }

        static LivingThing GenerateRandomCell(Random rand)
        {
            string[] randomNames = { "Grax", "Tuli", "Morv", "Keen", "Zovo", "Redd", "Jarn", "Plix", "Vor", "Koda" };
            string name = randomNames[rand.Next(randomNames.Length)];
            bool isCarnivore = rand.Next(2) == 0;

            LivingThing newCell = isCarnivore ? new Carnivore(name) : new Herbivore(name);
            newCell.Size = rand.Next(1, 8);
            return newCell;
        }

        static void EncounterCluster(LivingThing player, List<Cluster> clusters, Random rand)
        {
            if (rand.NextDouble() < 0.5 || clusters.Count == 0)
            {
                Cluster wildCluster = GenerateRandomCluster(rand);
                clusters.Add(wildCluster);
            }

            var availableClusters = clusters.Where(c => !c.Cells.Contains(player)).ToList();
            if (availableClusters.Count == 0)
            {
                Console.WriteLine("There are no new clusters to encounter right now.");
                return;
            }

            Cluster cluster = availableClusters[rand.Next(availableClusters.Count)];
            Console.WriteLine($"\nYou encountered Cluster '{cluster.Name}' with {cluster.Cells.Count} members.");

            Console.Write("Join it? (Y/N): ");
            ConsoleKey response = Console.ReadKey(true).Key;

            if (response == ConsoleKey.Y)
            {
                foreach (var c in clusters)
                    if (c.Cells.Contains(player))
                        c.RemoveCell(player);

                cluster.AddCell(player);
                Console.WriteLine($"{player.Name} joined Cluster '{cluster.Name}'.");
                Console.WriteLine("Hint: Cluster size is indicated by the numbers that follow the name of the cluster. Any cluster above size 50 allows you to evolve.");
            }
            else
            {
                Console.WriteLine($"{player.Name} decided not to join.");
            }
        }

        static Cluster GenerateRandomCluster(Random rand)
        {
            string[] clusterNames = { "Haven", "Nest", "Core", "Pod", "Hive", "Shelter", "Den", "Matrix" };
            string name = clusterNames[rand.Next(clusterNames.Length)] + rand.Next(1, 100);
            return new Cluster(name);
        }

        static bool CheckEvolution(LivingThing player, List<Cluster> clusters)
        {
            var cluster = clusters.FirstOrDefault(c => c.Cells.Contains(player));
            if (cluster == null) return false;

            int clusterNumber = ExtractClusterSize(cluster.Name);
            return player.Size >= 15 && clusterNumber > 50;
        }

        static int ExtractClusterSize(string clusterName)
        {
            string digits = new string(clusterName.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int num) ? num : 0;
        }

        static void HandleEvolution(ref LivingThing player)
        {
            Console.WriteLine($"\n{player.Name} is ready to evolve!");
            Console.WriteLine("Choose your evolution:");
            Console.WriteLine(" [1] Alderbrook (Carnivore: Health 2, Speed 1, Attack 2)");
            Console.WriteLine(" [2] Alien Didi (Omnivore: Speed 2, Attack 1)");
            Console.Write("Enter choice: ");

            string? input = Console.ReadLine();
            LivingThing evolved;

            if (input == "1")
            {
                evolved = new Alderbrook(player.Name) { Size = player.Size };
                Console.WriteLine($"{player.Name} evolved into Alderbrook!");
            }
            else if (input == "2")
            {
                evolved = new AlienDidi(player.Name) { Size = player.Size };
                Console.WriteLine($"{player.Name} evolved into Alien Didi!");
            }
            else
            {
                Console.WriteLine("Invalid choice. Evolution cancelled.");
                return;
            }

            player = evolved;
        }
    }

    abstract class Species : LivingThing
    {
        public int Health { get; protected set; }
        public int Speed { get; protected set; }
        public int Attack { get; protected set; }

        public Species(string name) : base(name) { }
    }

    class Alderbrook : Species
    {
        public Alderbrook(string name) : base(name)
        {
            Health = 2;
            Speed = 1;
            Attack = 2;
        }
    }

    class Omnivore : LivingThing
    {
        public Omnivore(string name) : base(name) { }

        public override void Eat(FoodType food)
        {
            Size++;
        }

        public override void Eat(LivingThing other)
        {
            if (this.Size <= other.Size)
                throw new InvalidOperationException($"{Name} is too small to eat {other.Name}.");

            int gained = other.Size;
            this.Size += gained;
            Console.WriteLine($"{Name} devoured {other.Name} and gained {gained} size.");
        }
    }

    class AlienDidi : Omnivore
    {
        public int Speed { get; private set; } = 2;
        public int Attack { get; private set; } = 1;

        public AlienDidi(string name) : base(name) { }
    }
}





