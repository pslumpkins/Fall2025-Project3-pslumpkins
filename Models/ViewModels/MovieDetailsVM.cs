namespace Fall2025_Project3_pslumpkins.Models.ViewModels
{
    public sealed class MovieDetailsVM
    {
        public Movie Movie { get; set; } = default;
        public List<Actor> Actors { get; set; } = new();
        public List<(string Review, double Compound)> Reviews { get; set; } = new();
        public double AverageCompound { get; set; }

    }
}
