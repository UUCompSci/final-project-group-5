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

        public override void Eat(FoodType food)
        {
            if (food == FoodType.Plant) Size++;
        }

        public override void Eat(LivingThing other)
        {
            if (Size > other.Size)
            {
                int gained = other.Size;
                Size += gained;
            }
        }
    }

    class AlienDidi : Omnivore
    {
        public int Speed { get; private set; } = 2;
        public int Attack { get; private set; } = 1;

        public AlienDidi(string name) : base(name) { }

        public override void Eat(FoodType food)
        {
            if (food == FoodType.Plant) Size++;
        }

        public override void Eat(LivingThing other)
        {
            if (Size > other.Size)
            {
                int gained = other.Size;
                Size += gained;
            }
        }
    }

    class GameHelpers
    {
        private static Random rand = new Random();

        public static LivingThing GenerateRandomCell()
        {
            string[] randomNames = { "Grax", "Tuli", "Morv", "Keen", "Zovo", "Redd", "Jarn", "Plix", "Vor", "Koda" };
            string name = randomNames[rand.Next(randomNames.Length)];
            bool isCarnivore = rand.Next(2) == 0;
            LivingThing newCell = isCarnivore ? new Carnivore(name) : new Herbivore(name);
            newCell.Size = rand.Next(1, 8);
            return newCell;
        }

        public static Cluster GenerateRandomCluster()
        {
            string[] clusterNames = { "Haven", "Nest", "Core", "Pod", "Hive", "Shelter", "Den", "Matrix" };
            string name = clusterNames[rand.Next(clusterNames.Length)] + rand.Next(1, 100);
            return new Cluster(name);
        }

        public static int ExtractClusterSize(string clusterName)
        {
            string digits = new string(clusterName.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int num) ? num : 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SaveLoad.EnsureDatabase();

            List<LivingThing> allCells = new List<LivingThing>();
            List<Cluster> clusters = new List<Cluster>();
            LivingThing? player = null;
            bool hasEvolved = false;

            CreateInitialPlayerCell(ref player, allCells);

            if (player != null)
            {
                for (int i = 0; i < 10; i++)
                    allCells.Add(GameHelpers.GenerateRandomCell());
            }

            bool running = true;
            ShowMenu();

            while (running)
            {
                Console.Write("\nPress a key: ");
                ConsoleKey key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.T:
                        if (player != null)
                        {
                            PlaySmartTurn(ref player, allCells, clusters, ref hasEvolved);
                        }
                        break;

                    case ConsoleKey.S:
                        if (player != null)
                        {
                            ShowStats(player, clusters);
                        }
                        break;

                    case ConsoleKey.V:
                        SaveLoad.SaveWorld(allCells, clusters, player);
                        break;

                    case ConsoleKey.L:
                        (allCells, clusters, player) = SaveLoad.LoadWorld();
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
        }

        static void ShowMenu()
        {
            Console.WriteLine("\nAvailable Keys:");
            Console.WriteLine(" [T] Advance Turn");
            Console.WriteLine(" [S] Stats");
            Console.WriteLine(" [V] Save World");
            Console.WriteLine(" [L] Load World");
            Console.WriteLine(" [Q] Quit");
        }

        static void CreateInitialPlayerCell(ref LivingThing? player, List<LivingThing> allCells)
        {
            Console.Write("\nEnter type for player cell (Carnivore/Herbivore): ");
            string? typeInput = Console.ReadLine()?.Trim().ToLower();
            if (typeInput != "carnivore" && typeInput != "herbivore") return;

            Console.Write("Enter name for player cell: ");
            string? name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            player = typeInput == "carnivore" ? new Carnivore(name) : new Herbivore(name);
            player.Size = 3;
            allCells.Add(player);
            Console.WriteLine($"Created {typeInput} named {player.Name} (Size: {player.Size})");
        }

        static void PlaySmartTurn(ref LivingThing player, List<LivingThing> allCells, List<Cluster> clusters, ref bool hasEvolved)
        {
            bool actionTaken = false;
            Random rand = new Random();
            LivingThing localPlayer = player;

            if (!hasEvolved && CheckEvolution(localPlayer, clusters))
            {
                HandleEvolution(ref player, allCells);
                hasEvolved = true;
                actionTaken = true;
                localPlayer = player;
            }

            double roll = rand.NextDouble();

            if (!actionTaken && roll < 0.3)
            {
                LivingThing newCell = GameHelpers.GenerateRandomCell();
                int attempts = 0;
                while (allCells.Any(c => c.Name == newCell.Name) && attempts < 10)
                {
                    newCell = GameHelpers.GenerateRandomCell();
                    attempts++;
                }
                if (allCells.Any(c => c.Name == newCell.Name))
                    newCell.Name += rand.Next(1000, 9999);

                allCells.Add(newCell);
                Console.WriteLine($"{newCell.Name} appeared in the world (Size: {newCell.Size}).");
                actionTaken = true;
            }

            if (!actionTaken && roll < 0.6)
            {
                var targets = allCells.Where(c => c != localPlayer && localPlayer.Size > c.Size).ToList();
                if (targets.Count > 0)
                {
                    LivingThing target = targets.OrderByDescending(c => c.Size).First();

                    if (localPlayer is Carnivore c)
                    {
                        c.Eat(target);
                        allCells.Remove(target);
                        Console.WriteLine($"{localPlayer.Name} ate {target.Name}. Size is now {localPlayer.Size}.");
                    }
                    else if (localPlayer is Omnivore o)
                    {
                        o.Eat(target);
                        allCells.Remove(target);
                        Console.WriteLine($"{localPlayer.Name} ate {target.Name}. Size is now {localPlayer.Size}.");
                    }
                    else if (localPlayer is Alderbrook a)
                    {
                        a.Eat(target);
                        allCells.Remove(target);
                        Console.WriteLine($"{localPlayer.Name} ate {target.Name}. Size is now {localPlayer.Size}.");
                    }
                    else if (localPlayer is Herbivore)
                    {
                        Console.WriteLine($"{localPlayer.Name} encountered {target.Name} but cannot eat it.");
                    }

                    actionTaken = true;
                }
            }

            if (!actionTaken && roll < 0.85)
            {
                FoodType food = rand.Next(2) == 0 ? FoodType.Plant : FoodType.Meat;
                Console.WriteLine($"\n{localPlayer.Name} encountered {food}.");
                try
                {
                    if (food == FoodType.Plant)
                        localPlayer.Size += rand.Next(1, 6);

                    if (!(localPlayer is Herbivore && food == FoodType.Meat))
                        localPlayer.Eat(food);

                    if (!(localPlayer is Herbivore && food == FoodType.Meat))
                        Console.WriteLine($"{localPlayer.Name} ate {food}. Size is now {localPlayer.Size}.");
                }
                catch { }
                actionTaken = true;
            }

            if (!actionTaken)
            {
                Cluster? currentCluster = clusters.FirstOrDefault(c => c.Cells.Contains(localPlayer));
                int currentClusterNumber = currentCluster != null ? GameHelpers.ExtractClusterSize(currentCluster.Name) : 0;

                var availableClusters = clusters
                    .Where(c => !c.Cells.Contains(localPlayer) && GameHelpers.ExtractClusterSize(c.Name) > currentClusterNumber)
                    .OrderByDescending(c => GameHelpers.ExtractClusterSize(c.Name))
                    .ToList();

                if (!availableClusters.Any())
                {
                    Cluster newCluster = GameHelpers.GenerateRandomCluster();
                    for (int i = 0; i < rand.Next(2, 6); i++)
                        newCluster.AddCell(GameHelpers.GenerateRandomCell());
                    clusters.Add(newCluster);
                    availableClusters.Add(newCluster);
                }

                Cluster chosenCluster = availableClusters.First();
                if (currentCluster != null) currentCluster.RemoveCell(localPlayer);
                chosenCluster.AddCell(localPlayer);

                Console.WriteLine($"{localPlayer.Name} joined Cluster '{chosenCluster.Name}' (Cluster size: {GameHelpers.ExtractClusterSize(chosenCluster.Name)})");
                Console.WriteLine("Hint: Cluster size is indicated by the numbers that follow the name of the cluster. Any cluster above size 50 allows you to evolve.");
                actionTaken = true;
            }

            if (!actionTaken)
                Console.WriteLine($"{localPlayer.Name} is staring into their own reflection...");
        }

        static void ShowStats(LivingThing player, List<Cluster> clusters)
        {
            Console.WriteLine($"\n{player.Name} - Size: {player.Size}, Type: {player.GetType().Name}");
            int health = 1, attack = 1, speed = 1;

            if (player is Species s) { health = s.Health; attack = s.Attack; speed = s.Speed; }
            else if (player is AlienDidi ad) { attack = ad.Attack; speed = ad.Speed; health = player.Size; }

            Console.WriteLine($"Stats -> Health: {health}, Attack: {attack}, Speed: {speed}");

            var playerClusters = clusters.Where(c => c.Cells.Contains(player)).ToList();

            if (playerClusters.Count == 0) Console.WriteLine("Clusters: None");
            else Console.WriteLine("Clusters: " + string.Join(", ", playerClusters.Select(c => $"{c.Name} (Cluster size: {GameHelpers.ExtractClusterSize(c.Name)})")));
        }

        static bool CheckEvolution(LivingThing player, List<Cluster> clusters)
        {
            var cluster = clusters.FirstOrDefault(c => c.Cells.Contains(player));
            if (cluster == null) return false;

            int clusterNumber = GameHelpers.ExtractClusterSize(cluster.Name);
            return player.Size >= 15 && clusterNumber > 50;
        }

        static void HandleEvolution(ref LivingThing player, List<LivingThing> allCells)
        {
            Console.WriteLine($"\n{player.Name} is ready to evolve!");
            Random rand = new Random();
            int choice = rand.Next(1, 3);
            int index = allCells.IndexOf(player);

            string oldName = player.Name;
            LivingThing evolvedPlayer;

            if (choice == 1) evolvedPlayer = new Alderbrook(player.Name) { Size = player.Size };
            else evolvedPlayer = new AlienDidi(player.Name) { Size = player.Size };

            string typeIndicator = evolvedPlayer is Alderbrook ? "(Carnivore)" :
                                   evolvedPlayer is AlienDidi ? "(Omnivore)" : "(Herbivore)";

            Console.WriteLine($"{oldName} has evolved into {evolvedPlayer.GetType().Name} {typeIndicator}!");

            player = evolvedPlayer;
            if (index >= 0) allCells[index] = player;
        }
    }
}
