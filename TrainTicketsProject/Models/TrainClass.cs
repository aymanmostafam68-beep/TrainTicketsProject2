namespace TrainTicketsProject.Models
{
    public  class TrainClass : BaseClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public double IncreasePercentage { get; set; } = 0;

        public bool Hasfood { get; set; } = false;

        public bool HasWifi { get; set; } = false;

        public bool HasAirConditioning { get; set; } = false;
           
        public virtual ICollection<Carriage> Carriages { get; set; } = new List<Carriage>();


    }
}

