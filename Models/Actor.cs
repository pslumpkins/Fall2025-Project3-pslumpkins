using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_pslumpkins.Models
{
    public class Actor
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(50)]
        public string? Gender { get; set; } = "";

        [Range(0, 120)]
        public int? Age { get; set; }

        [Url] public string? ActorImdb { get; set; }

        public byte[]? Photo { get; set; }

        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>(); 

    }
}
