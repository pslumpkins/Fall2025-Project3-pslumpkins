namespace Fall2025_Project3_pslumpkins.Models.ViewModels
{
    public sealed class ActorDetailsVM
    {
        public Actor Actor { get; set; } = default!;
        public List<Movie> Movies { get; set; } = new();
        public List<(string Tweet, double Compound)> Tweets { get; set; } = new();
        public double AverageCompound { get; set; }
    }
}
