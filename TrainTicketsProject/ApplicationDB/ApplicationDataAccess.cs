
using TrainTicketsProject.Models;
namespace TrainTicketsProject.ApplicationDB
{
    public class ApplicationDataAccess : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDataAccess(DbContextOptions<ApplicationDataAccess> options) : base(options)
        {
        }
        public DbSet<GeneralSetting>  generalSettings { get; set; }
        public DbSet<Station> stations { get; set; }
        public DbSet<Route> routes { get; set; }
        public DbSet<Train>  trains { get; set; }
        public DbSet<Carriage>  Carriages { get; set; }
        public DbSet<CarriageSeat> carriageSeats { get; set; }
        public DbSet<Booking>   bookings { get; set; }
        public DbSet<Transaction> transactions { get; set; }
        public DbSet<TransactionEntry> transactionEntries { get; set; }
        public DbSet<TrainClass>  trainClasses { get; set; }
        public DbSet<PaymentMethod> paymentMethods { get; set; }
        public DbSet<TripSchedule> tripSchedules { get; set; }
        public DbSet<Trip> trips { get; set; }

        public DbSet<RouteStation> RouteStations { get; set; }
        public DbSet<DepartureTimeInterval> DepartureTimeIntervals { get; set; }



        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //TripSchedule


            modelBuilder.Entity<TripSchedule>()
                .HasOne(ts => ts.Route)
                .WithMany()
                .HasForeignKey(ts => ts.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripSchedule>()
                .HasOne(ts => ts.Train)
                .WithMany()
                .HasForeignKey(ts => ts.TrainId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripSchedule>()
                .HasOne(ts => ts.ReturnSchedule)
                .WithMany()
                .HasForeignKey(ts => ts.ReturnScheduleId)
                .OnDelete(DeleteBehavior.Restrict);



            // Route

            modelBuilder.Entity<Route>()
                    .HasIndex(c => c.Code)
                    .IsUnique();

            // station

            modelBuilder.Entity<Station>()
                    .HasIndex(c => c.Code)
                    .IsUnique();

       
            // train

            modelBuilder.Entity<Train>()
                    .HasIndex(c => c.TrainCode)
                    .IsUnique();


            //RouteStation
            modelBuilder.Entity<RouteStation>()
       .HasIndex(rs => new { rs.RouteId, rs.StationId })
       .IsUnique();

            modelBuilder.Entity<RouteStation>()
                .HasOne(rs => rs.Route)
                .WithMany(r => r.RouteStations)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Cascade);  

            modelBuilder.Entity<RouteStation>()
                .HasOne(rs => rs.Station)
                .WithMany(s => s.RouteStations)
                .HasForeignKey(rs => rs.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStation>()
    .Property(rs => rs.DistanceFromStart)
    .HasPrecision(18, 2);


            //booking
            modelBuilder.Entity<Booking>()
    .HasOne(b => b.Carriage)
    .WithMany()
    .HasForeignKey(b => b.CarriageId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.CarriageSeat)
                .WithMany()
                .HasForeignKey(b => b.CarriageSeatId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ReturnCarriage)
                .WithMany()
                .HasForeignKey(b => b.ReturnCarriageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ReturnCarriageSeat)
                .WithMany()
                .HasForeignKey(b => b.ReturnCarriageSeatId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TripSchedule)
                .WithMany()
                .HasForeignKey(b => b.TripScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ReturnTripSchedule)
                .WithMany()
                .HasForeignKey(b => b.ReturnTripScheduleId)
                .OnDelete(DeleteBehavior.Restrict);







        }


    }
}
