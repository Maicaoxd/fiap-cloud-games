using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Games
{
    public sealed record UpdateGameRequest(
        [Required(ErrorMessage = ApiMessages.Game.TitleRequired)]
        string Title,
        [Required(ErrorMessage = ApiMessages.Game.DescriptionRequired)]
        string Description,
        [Range(0, double.MaxValue, ErrorMessage = ApiMessages.Game.PriceCannotBeNegative)]
        decimal Price);
}
