using System;
/*
namespace WorldSimulator
{
    interface ILiving
    {
        string Name { get; }
        double Energy { get; }
        bool Alive { get; }

        void Live();
        void Eat(double amount);
        bool CanEvolve();
        ILiving Evolve();
    }

    interface IAction
    {
        void Act();
    }

    abstract class LifeForm : ILiving
    {
        public string Name { get; protected set; }
        public double Energy { get; protected set; }
        public bool Alive { get; protected set; } = true;

        protected static Random rand = new Random();

        protected LifeForm(string name, double energy)
        {
            Name = name;
            Energy = energy;
        }

        public virtual void Live()
        {
            if (!Alive) return;
            double recovery = rand.NextDouble() * 2;
            Energy += recovery - 0.5;
            Console.WriteLine($"{Name} rests. (+{recovery:F1} energy, total: {Energy:F1})");
            if (rand.NextDouble() < 0.05)
            {
                double loss = rand.NextDouble() * 3;
                Energy -= loss;
                Console.WriteLine($"{Name} was attacked while resting (-{loss:F1} energy).");
            }
            CheckAlive();
        }

        public virtual void Eat(double amount)
        {
            if (!Alive) return;
            Energy += amount;
            Console.WriteLine($"{Name} eats and gains {amount:F1} energy (total: {Energy:F1}).");
        }

        protected void CheckAlive()
        {
            if (Energy <= 0)
            {
                Alive = false;
                Console.WriteLine($"{Name} has died from exhaustion...");
            }
        }

        public virtual bool CanEvolve() => false;
        public virtual ILiving Evolve() => this;
    }

    class Cell : LifeForm, IAction
    {
        private int divisions = 0;

        public Cell() : base("Cell", 5) { }

        public void Act()
        {
            if (Energy > 5)
            {
                Energy -= 2;
                divisions++;
                Console.WriteLine($"{Name} divides! Total divisions: {divisions}");
            }
            else
            {
                Energy -= 1;
                Console.WriteLine($"{Name} struggles to divide and weakens...");
            }

            if (rand.NextDouble() < 0.1 && Energy < 3)
            {
                Alive = false;
                Console.WriteLine($"{Name} ruptures during division and dies.");
            }
            CheckAlive();
        }

        public override bool CanEvolve() => Energy >= 10 && divisions >= 2;
        public override ILiving Evolve()
        {
            if (CanEvolve())
                return new Multicellular();
            return this;
        }
    }

    class Multicellular : LifeForm, IAction
    {
        private int cellCount;

        public Multicellular() : base("Multicellular Organism", 15)
        {
            cellCount = rand.Next(10, 100);
        }

        public void Act()
        {
            int growth = rand.Next(2, 6);
            cellCount += growth;
            Energy -= growth * 0.5;
            Console.WriteLine($"{Name} grows to {cellCount} cells (energy: {Energy:F1}).");
            if (cellCount > 150 && rand.NextDouble() < 0.2)
            {
                Alive = false;
                Console.WriteLine($"{Name}'s cells overgrow uncontrollably... It collapses.");
            }
            CheckAlive();
        }

        public override bool CanEvolve() => Energy >= 25 && cellCount > 80;
        public override ILiving Evolve()
        {
            if (CanEvolve())
                return new Animal();
            return this;
        }
    }

    class Animal : LifeForm, IAction
    {
        private int strength;

        public Animal() : base("Animal", 25)
        {
            strength = rand.Next(3, 7);
        }

        public void Act()
        {
            Console.WriteLine($"{Name} hunts...");
            if (rand.NextDouble() < 0.7)
            {
                double food = rand.NextDouble() * 6 + 4;
                Eat(food);
                strength += 1;
                Console.WriteLine($"{Name} hunts successfully! (+{food:F1} energy, +1 strength, strength: {strength})");
            }
            else
            {
                double damage = rand.NextDouble() * 4;
                Energy -= damage;
                Console.WriteLine($"{Name} is injured while hunting (-{damage:F1} energy, strength: {strength})");
            }

            if (Energy < 5 && rand.NextDouble() < 0.1)
            {
                Alive = false;
                Console.WriteLine($"{Name} starved to death while searching for prey.");
            }
            CheckAlive();
        }

        public override bool CanEvolve() => Energy >= 30 && strength >= 7;
        public override ILiving Evolve()
        {
            if (CanEvolve())
                return new SentientBeing();
            return this;
        }

        public int GetStrength() => strength;
    }

    class SentientBeing : LifeForm, IAction
    {
        private int intelligence = 100;

        public SentientBeing() : base("Sentient Being", 40) { }

        public void Act()
        {
                Console.WriteLine("Choose intellectual action: (T)hink or (C)reate");
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.C && Energy > 2)
                {
                        Energy -= 2;
                        intelligence += 2;
                        Console.WriteLine($"{Name} creates (energy: {Energy:F1}, intelligence: {intelligence}).");
                }
                else
                {
                        Energy -= 1;
                        intelligence++;
                        Console.WriteLine($"{Name} thinks (energy: {Energy:F1}, intelligence: {intelligence}).");
                }

                if (intelligence > 200 && rand.NextDouble() < 0.1)
                {
                        Alive = false;
                        Console.WriteLine($"{Name} overthinks reality and loses the will to live...");
                }

                CheckAlive();
        }


        public int GetIntelligence() => intelligence;
    }

    class WorldSimulator
    {
        private ILiving entity;
        private double availableFood = 10;
        private int turnsWithoutFood = 0;
        private double maxFood = 10;
        private Random rand = new Random();

        public WorldSimulator()
        {
            entity = new Cell();
        }

        public void Run()
        {
            while (entity.Alive)
            {
                Console.WriteLine($"\n--- {entity.Name}'s Turn ---");
                string hud = $"Energy: {entity.Energy:F1} | Food: {availableFood:F1}";

                if (entity is Animal a)
                    hud += $" | Strength: {a.GetStrength()}";
                if (entity is SentientBeing s)
                    hud += $" | Intelligence: {s.GetIntelligence()}";

                Console.WriteLine(hud);
                ShowOptions();

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        TryEat();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        if (entity is IAction actor)
                            actor.Act();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        entity.Live();
                        RegenerateFood();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        TryEvolve();
                        break;
                    case ConsoleKey.Q:
                        return;
                }
                if (!entity.Alive) break;
            }

            Console.WriteLine($"\nGAME OVER. You reached {entity.Name} with {entity.Energy:F1} energy.");
        }

        private void ShowOptions()
        {
            if (availableFood > 0)
                Console.WriteLine("[1] Eat");
            else
                Console.WriteLine("[1] Eat (no food available)");
            Console.WriteLine("[2] Perform special action");
            Console.WriteLine("[3] Rest (Live)");
            Console.WriteLine("[4] Attempt to evolve");
            Console.WriteLine("[Q] Quit");
        }

        private void TryEat()
        {
            if (availableFood <= 0)
            {
                Console.WriteLine("No food available, rest to let it regrow.");
                return;
            }
            double foodEaten = Math.Min(3, availableFood);
            availableFood -= foodEaten;
            entity.Eat(foodEaten);
            turnsWithoutFood = 0;
        }

        private void RegenerateFood()
        {
            turnsWithoutFood++;
            if (turnsWithoutFood >= 2)
            {
                double regen = Math.Min(maxFood - availableFood, rand.NextDouble() * 3 + 1);
                availableFood += regen;
                turnsWithoutFood = 0;
            }
        }

        private void TryEvolve()
        {
            var evolved = entity.Evolve();
            if (evolved != entity)
            {
                entity = evolved;
                if (entity is Multicellular)
                    maxFood = 15;
                else if (entity is Animal)
                    maxFood = 18;
                else if (entity is SentientBeing)
                    maxFood = 25;
            }
            else
            {
                Console.WriteLine("Cannot evolve yet. Keep growing and improving your stats.");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var sim = new WorldSimulator();
            sim.Run();
        }
    }
}

*/