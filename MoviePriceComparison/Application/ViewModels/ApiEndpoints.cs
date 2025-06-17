namespace MoviePriceComparison.Application.DTOs
{
    public class ApiEndpoints
    {
        public string Movies { get; set; } = "/movies";
        public string MovieDetail { get; set; } = "/movie/{id}";
    }
}
