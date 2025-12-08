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

    public static void SaveWorld(List<LivingThing> allCells, List<Cluster> clusters, LivingThing player)
    {
        using var db = new WorldContext();

        db.Database.EnsureCreated();

        db.Cells.RemoveRange(db.Cells);
        db.Clusters.RemoveRange(db.Clusters);
        db.SaveChanges();

        var originalName = player.Name;
        player.Name = "__PLAYER__" + player.Name;

        db.Clusters.AddRange(clusters);
        db.Cells.AddRange(allCells);

        db.SaveChanges();
        Console.WriteLine("World saved to database.");

        player.Name = originalName;
    }

    public static (List<LivingThing>, List<Cluster>, LivingThing?) LoadWorld()
    {
        using var db = new WorldContext();
        db.Database.EnsureCreated();

        var cells = db.Cells.ToList();
        var clusters = db.Clusters.Include(c => c.Cells).ToList();

        LivingThing? player = cells.FirstOrDefault(c => c.Name.StartsWith("__PLAYER__"));
        if (player != null)
        {
            player.Name = player.Name.Replace("__PLAYER__", "");
            Console.WriteLine($"Player cell '{player.Name}' restored successfully.");
        }
        else
        {
            Console.WriteLine("No player cell found in save data. Defaulting to first cell.");
            player = cells.FirstOrDefault();
        }

        Console.WriteLine("World loaded from database.");
        return (cells, clusters, player);
    }
}
