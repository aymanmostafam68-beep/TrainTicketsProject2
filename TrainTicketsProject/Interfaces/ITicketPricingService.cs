namespace TrainTicketsProject.Interfaces
{
    public interface ITicketPricingService
    {
        decimal CalculatePrice(decimal distance, TrainClass trainClass, TicketType ticketType, GeneralSetting generalSetting);

    }
}
