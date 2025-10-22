using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_pslumpkins.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required, StringLength(200)] public string Title { get; set; } = "";
        [Url] public string? MovieImdb { get; set; }
        [StringLength(100)] public string? Genre { get; set; }
        [Range(1888, 2100)] public int? Year { get; set; }

        public byte[]? Poster { get; set; }

        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();

    }
}
