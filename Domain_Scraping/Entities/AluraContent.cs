namespace Domain_Scraping.Entities
{
    public class AluraContent : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Link { get; set; } = null!;
        public string Professores { get; set; } = null!;
        public string Duration { get; set; } = null!;
    }
}
