#pragma warning disable 1570

namespace SCSSdkClient.Object {
    public partial class SCSTelemetry {
        /// <summary>
        ///     Car job values. Income, destination, source, cargo, and customer priorities.
        /// </summary>
        internal class CarJob {
            public CarJob() {
                DeliveryTime = new Time();
                RemainingDeliveryTime = new Frequency();
                CargoValues = new Cargo();
            }

            public Time DeliveryTime { get; internal set; }
            public Frequency RemainingDeliveryTime { get; protected internal set; }
            public bool CargoLoaded { get; internal set; }
            public bool CustomerPrioCargoHandling { get; internal set; }
            public bool CustomerPrioTime { get; internal set; }
            public bool CustomerPrioVehicleAppearance { get; internal set; }
            public string Market { get; internal set; }
            public uint PlannedDistanceKm { get; internal set; }
            public Cargo CargoValues { get; internal set; }
            public string CityDestinationId { get; internal set; }
            public string CityDestination { get; internal set; }
            public string CompanyDestinationId { get; internal set; }
            public string CompanyDestination { get; internal set; }
            public string CitySourceId { get; internal set; }
            public string CitySource { get; internal set; }
            public string CompanySourceId { get; internal set; }
            public string CompanySource { get; internal set; }
            public ulong Income { get; internal set; }

            public class Cargo {
                public float Mass { get; internal set; }
                public string Id { get; internal set; }
                public string Name { get; internal set; }
                public uint UnitCount { get; internal set; }
                public float UnitMass { get; internal set; }
                public float CargoDamage { get; internal set; }
            }
        }
    }
}
