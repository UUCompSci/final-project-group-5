using Entities;
using DataModel;
using Microsoft.EntityFrameworkCore;

public static class SaveLoad
{
    public static void EnsureDatabase()
    {
        using var db = new WorldContext();
        db.Database.EnsureCreated();
    }
    public static void SaveWorld(List<LivingThing> allCells, List<Cluster> clusters)
    {
        using var db = new WorldContext();

        db.Database.EnsureCreated();

        db.Cells.RemoveRange(db.Cells);
        db.Clusters.RemoveRange(db.Clusters);
        db.SaveChanges();

        db.Clusters.AddRange(clusters);
        db.Cells.AddRange(allCells);

        db.SaveChanges();
        Console.WriteLine("World saved to database.");
    }

    public static (List<LivingThing>, List<Cluster>) LoadWorld()
    {
        using var db = new WorldContext();
        db.Database.EnsureCreated();

        var cells = db.Cells.Include(c => c.Id).ToList();
        var clusters = db.Clusters.Include(c => c.Cells).ToList();

        Console.WriteLine("World loaded from database.");
        return (cells, clusters);
    }
}
