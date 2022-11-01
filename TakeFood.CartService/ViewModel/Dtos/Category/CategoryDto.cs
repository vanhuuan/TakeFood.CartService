using System.Text.Json.Serialization;

namespace CartService.ViewModel.Dtos.Category
{
    public class CategoryDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
