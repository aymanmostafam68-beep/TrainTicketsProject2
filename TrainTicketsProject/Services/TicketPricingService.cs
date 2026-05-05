namespace TrainTicketsProject.Services
{
    public class TicketPricingService : ITicketPricingService
    {
        public decimal CalculatePrice(decimal distance, TrainClass trainClass, TicketType ticketType, GeneralSetting generalSetting)
        {
            decimal basePrice = distance * generalSetting.PricePerKilometer;

            decimal classPrice = basePrice + (basePrice * ((decimal)trainClass.IncreasePercentage / 100m));

            decimal finalPrice = ticketType switch
            {
                TicketType.OneWay => classPrice,
                TicketType.RoundTrip => classPrice * 2
            };

            return finalPrice;
        }
    }
}
