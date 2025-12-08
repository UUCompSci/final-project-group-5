namespace Entities;

// BASE ENTITY
public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public BaseEntity() { }
}

// BASE CLASS FOR ALL LIVING THINGS
public class LivingThing : BaseEntity
{
    public LivingThing(string name)
    {
        Name = name;
        Size = 1;
    }

    public string Name { get; set; }
    public int Size { get; set; }

    // Found food 
    public virtual void Eat(FoodType food)
    {
        throw new InvalidOperationException($"{Name} cannot eat that.");
    }

    // To be otherthrown by carnivore
    public virtual void Eat(LivingThing other)
    {
        throw new InvalidOperationException($"{Name} cannot eat other creatures.");
    }
}

// HERBIVORE
public class Herbivore : LivingThing
{
    public Herbivore(string name) : base(name) { }

    public override void Eat(FoodType food)
    {
        if (food != FoodType.Plant)
            throw new InvalidOperationException($"{Name} can only eat plants");

        Size++;
    }
}

// CARNIVORE
public class Carnivore : LivingThing
{
    public Carnivore(string name) : base(name) { }

    // Found food
    public override void Eat(FoodType food)
    {
        if (food != FoodType.Meat)
            throw new InvalidOperationException($"{Name} can only eat meat");

        Size++;
    }

    // Other creature - creature's size added to yours.
    public override void Eat(LivingThing other)
    {
        if (this.Size <= other.Size)
            throw new InvalidOperationException(
                $"{Name} is not large enough to eat {other.Name}."
            );

        int gained = other.Size; // Amount gained from eating
        this.Size += gained;

        Console.WriteLine($"{Name} ate {other.Name} and gained {gained} size!");
    }
}

// OMNIVORE
public class Omnivore : LivingThing
{
    public Omnivore(string name) : base(name) { }

    // Found food
    public override void Eat(FoodType food)
    {
        Size++;
    }

    // Other creature - creature's size added to yours.
    public override void Eat(LivingThing other)
    {
        if (this.Size <= other.Size)
            throw new InvalidOperationException(
                $"{Name} is not large enough to eat {other.Name}."
            );

        int gained = other.Size; // Amount gained from eating
        this.Size += gained;

        Console.WriteLine($"{Name} ate {other.Name} and gained {gained} size!");
    }
}

// EATING
public enum FoodType
{
    Plant,
    Meat
}

// CLUSTER - (HOME)
public class Cluster : BaseEntity
{
    public Cluster(string name)
    {
        Name = name;
        Cells = new List<LivingThing>();
    }

    public string Name { get; set; }

    // All herbivores and carnivores inside this cluster
    public List<LivingThing> Cells { get; private set; }

    // Add a cell to the cluster
    public void AddCell(LivingThing cell)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        Cells.Add(cell);
    }

    // Remove a living thing from the cluster
    public void RemoveCell(LivingThing cell)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        Cells.Remove(cell);
    }
}

// Next step is create the program where things can interact and be created etc etc.
