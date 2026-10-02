using System;
using System.Text;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace SCSSdkClient.Tests {
    internal static class Program {
        private const int MapSize = 32 * 1024;
        private const int ExtensionOffset = 21600;
        private const int StringSize = 64;

        private static int Main() {
            AssertTrue(typeof(SCSTelemetry).GetProperty("CarJobValues") == null,
                "car job values is not public");
            AssertTrue(typeof(SCSTelemetry.GamePlayEvents).GetField("CarJobCancelled") == null,
                "car job cancelled gameplay object is not public");
            AssertTrue(typeof(SCSTelemetry.GamePlayEvents).GetField("CarJobDelivered") == null,
                "car job delivered gameplay object is not public");
            AssertTrue(typeof(SCSTelemetry.SpecialEvents).GetProperty("CarJobCancelled") == null,
                "car job cancelled special event is not public");
            AssertTrue(typeof(SCSTelemetry.SpecialEvents).GetProperty("CarJobDelivered") == null,
                "car job delivered special event is not public");
            AssertTrue(typeof(SCSSdkTelemetry).GetEvent("CarJobCancelled") == null,
                "car job cancelled event is not public");
            AssertTrue(typeof(SCSSdkTelemetry).GetEvent("CarJobDelivered") == null,
                "car job delivered event is not public");

            var data = new byte[MapSize];
            WriteLegacyCarJobValues(data);
            WriteExtensionZone(data, JobType.Car);

            var converter = new SCSSdkConvert();
            var telemetry = converter.Convert(data);

            AssertEqual(45, telemetry.CommonValues.NextMandatoryBreak.Value, "mandatory break");
            AssertTrue(telemetry.SpecialEventsValues.OnJob, "car job on job");
            AssertEqual(JobType.Car, telemetry.SpecialEventsValues.ActiveJobType, "active job type");
            AssertEqual((uint)620, telemetry.JobValues.DeliveryTime.Value, "unified job delivery time");
            AssertEqual((uint)77, telemetry.GamePlay.JobDelivered.DeliveryTime.Value, "unified job delivered time");
            AssertEqual((uint)2, telemetry.JobValues.CargoValues.UnitCount, "unified job unit count");
            AssertEqual((uint)815, telemetry.JobValues.PlannedDistanceKm, "unified job planned distance");
            AssertEqual(1234, telemetry.GamePlay.JobDelivered.EarnedXp, "legacy job earned xp");
            AssertEqual((uint)100, telemetry.GamePlay.JobCancelled.Started.Value, "unified job cancelled started");
            AssertEqual((uint)177, telemetry.GamePlay.JobDelivered.Finished.Value, "unified job delivered finished");
            AssertEqual(0.12f, telemetry.JobValues.CargoValues.CargoDamage, "unified job cargo damage");
            AssertEqual(0.34f, telemetry.GamePlay.JobDelivered.VehicleDamage, "unified job vehicle damage");
            AssertEqual(0.22f, telemetry.GamePlay.JobDelivered.CargoDamage, "unified job delivered cargo damage");
            AssertEqual(550.0f, telemetry.GamePlay.JobDelivered.DistanceKm, "unified job delivered distance");
            AssertEqual("quick_job", telemetry.JobValues.MarketName, "unified job market name");
            AssertEqual("cars", telemetry.JobValues.CargoValues.Id, "unified job cargo id");
            AssertEqual(9000UL, telemetry.JobValues.Income, "unified job income");
            AssertEqual(-500L, telemetry.GamePlay.JobCancelled.Penalty, "legacy job cancel penalty");
            AssertEqual(8700L, telemetry.GamePlay.JobDelivered.Revenue, "legacy job revenue");
            AssertTrue(telemetry.JobValues.CustomerPrioTime, "unified job customer time priority");
            AssertTrue(telemetry.SpecialEventsValues.JobCancelled, "legacy job cancelled flag");
            AssertTrue(telemetry.SpecialEventsValues.JobDelivered, "legacy job delivered flag");

            var ordinaryDeliveryData = (byte[])data.Clone();
            WriteOnJob(ordinaryDeliveryData, false);
            ordinaryDeliveryData[4303] = 0;
            var ordinaryDeliveryTelemetry = converter.Convert(ordinaryDeliveryData);
            AssertEqual(0.0f, ordinaryDeliveryTelemetry.GamePlay.JobDelivered.VehicleDamage,
                "ordinary job does not inherit car vehicle damage");

            var freightData = new byte[MapSize];
            WriteExtensionZone(freightData, JobType.Freight);
            WriteLegacyCarJobValues(freightData);
            var freightDeliveryOffset = 88;
            WriteUInt(freightData, ref freightDeliveryOffset, 300);
            var freightTelemetry = new SCSSdkConvert().Convert(freightData);
            AssertTrue(freightTelemetry.SpecialEventsValues.OnJob, "freight job on job");
            AssertEqual(JobType.Freight, freightTelemetry.SpecialEventsValues.ActiveJobType, "freight job type");
            AssertEqual((uint)300, freightTelemetry.JobValues.DeliveryTime.Value, "freight job does not inherit car job values");

            var legacyData = new byte[MapSize];
            WriteLegacyCarJobValues(legacyData);
            WriteExtensionZone(legacyData, JobType.None);
            var legacyTelemetry = new SCSSdkConvert().Convert(legacyData);
            AssertTrue(legacyTelemetry.SpecialEventsValues.OnJob, "legacy job on job");
            AssertEqual(JobType.Freight, legacyTelemetry.SpecialEventsValues.ActiveJobType, "legacy job type fallback");

            var emptyData = new byte[MapSize];
            WriteExtensionZone(emptyData, JobType.Car);
            WriteOnJob(emptyData, false);
            var emptyTelemetry = new SCSSdkConvert().Convert(emptyData);
            AssertTrue(!emptyTelemetry.SpecialEventsValues.OnJob, "empty job on job");
            AssertEqual(JobType.None, emptyTelemetry.SpecialEventsValues.ActiveJobType, "empty job type");
            AssertEqual((uint)0, emptyTelemetry.JobValues.DeliveryTime.Value, "empty job does not inherit car job values");

            return 0;
        }

        private static void WriteExtensionZone(byte[] data, JobType jobType) {
            var offset = ExtensionOffset;

            WriteInt(data, ref offset, 45);
            WriteInt(data, ref offset, 1234);

            WriteUInt(data, ref offset, 620);
            WriteUInt(data, ref offset, 2);
            WriteUInt(data, ref offset, 815);
            WriteUInt(data, ref offset, 77);
            WriteUInt(data, ref offset, 100);
            WriteUInt(data, ref offset, 177);

            WriteFloat(data, ref offset, 12.5f);
            WriteFloat(data, ref offset, 3.75f);
            WriteFloat(data, ref offset, 0.12f);
            WriteFloat(data, ref offset, 0.22f);
            WriteFloat(data, ref offset, 0.34f);
            WriteFloat(data, ref offset, 550.0f);

            WriteBool(data, ref offset, true);
            WriteBool(data, ref offset, false);
            WriteBool(data, ref offset, true);
            WriteBool(data, ref offset, false);
            WriteBool(data, ref offset, true);
            WriteBool(data, ref offset, true);

            WriteString(data, ref offset, "cars", StringSize);
            WriteString(data, ref offset, "Cars", StringSize);
            WriteString(data, ref offset, "praha", StringSize);
            WriteString(data, ref offset, "Praha", StringSize);
            WriteString(data, ref offset, "dealer.prg", StringSize);
            WriteString(data, ref offset, "Prague Dealer", StringSize);
            WriteString(data, ref offset, "berlin", StringSize);
            WriteString(data, ref offset, "Berlin", StringSize);
            WriteString(data, ref offset, "auction.ber", StringSize);
            WriteString(data, ref offset, "Berlin Auction", StringSize);
            WriteString(data, ref offset, "quick_job", 32);

            Align(ref offset, 8);
            WriteULong(data, ref offset, 9000UL);
            WriteLong(data, ref offset, -500L);
            WriteLong(data, ref offset, 8700L);
            WriteUInt(data, ref offset, (uint)jobType);
        }

        private static void WriteLegacyCarJobValues(byte[] data) {
            var offset = 88;
            WriteUInt(data, ref offset, 620);
            offset = 96;
            WriteUInt(data, ref offset, 2);
            offset = 100;
            WriteUInt(data, ref offset, 815);
            offset = 440;
            WriteUInt(data, ref offset, 77);
            offset = 444;
            WriteUInt(data, ref offset, 100);
            offset = 448;
            WriteUInt(data, ref offset, 177);

            offset = 640;
            WriteInt(data, ref offset, 1234);

            offset = 748;
            WriteFloat(data, ref offset, 12.5f);
            offset = 944;
            WriteFloat(data, ref offset, 3.75f);
            offset = 1456;
            WriteFloat(data, ref offset, 0.22f);
            offset = 1460;
            WriteFloat(data, ref offset, 550.0f);

            offset = 1564;
            WriteBool(data, ref offset, true);

            offset = 2556;
            WriteString(data, ref offset, "cars", StringSize);
            WriteString(data, ref offset, "Cars", StringSize);
            WriteString(data, ref offset, "praha", StringSize);
            WriteString(data, ref offset, "Praha", StringSize);
            WriteString(data, ref offset, "dealer.prg", StringSize);
            WriteString(data, ref offset, "Prague Dealer", StringSize);
            WriteString(data, ref offset, "berlin", StringSize);
            WriteString(data, ref offset, "Berlin", StringSize);
            WriteString(data, ref offset, "auction.ber", StringSize);
            WriteString(data, ref offset, "Berlin Auction", StringSize);
            WriteString(data, ref offset, "quick_job", 32);

            offset = 4000;
            WriteULong(data, ref offset, 9000UL);
            offset = 4200;
            WriteLong(data, ref offset, -500L);
            WriteLong(data, ref offset, 8700L);

            offset = 4300;
            WriteBool(data, ref offset, true);
            WriteBool(data, ref offset, true);
            WriteBool(data, ref offset, true);
            WriteBool(data, ref offset, true);

        }

        private static void WriteOnJob(byte[] data, bool value) {
            data[4300] = value ? (byte)1 : (byte)0;
        }

        private static void WriteBool(byte[] data, ref int offset, bool value) {
            data[offset] = value ? (byte)1 : (byte)0;
            offset++;
        }

        private static void WriteFloat(byte[] data, ref int offset, float value) {
            Align(ref offset, 4);
            WriteBytes(data, ref offset, BitConverter.GetBytes(value));
        }

        private static void WriteInt(byte[] data, ref int offset, int value) {
            Align(ref offset, 4);
            WriteBytes(data, ref offset, BitConverter.GetBytes(value));
        }

        private static void WriteLong(byte[] data, ref int offset, long value) {
            WriteBytes(data, ref offset, BitConverter.GetBytes(value));
        }

        private static void WriteUInt(byte[] data, ref int offset, uint value) {
            Align(ref offset, 4);
            WriteBytes(data, ref offset, BitConverter.GetBytes(value));
        }

        private static void WriteULong(byte[] data, ref int offset, ulong value) {
            WriteBytes(data, ref offset, BitConverter.GetBytes(value));
        }

        private static void WriteString(byte[] data, ref int offset, string value, int length) {
            var bytes = Encoding.UTF8.GetBytes(value);
            Array.Copy(bytes, 0, data, offset, Math.Min(bytes.Length, length));
            offset += length;
        }

        private static void WriteBytes(byte[] data, ref int offset, byte[] bytes) {
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            offset += bytes.Length;
        }

        private static void Align(ref int offset, int size) {
            while (offset % size != 0) {
                offset++;
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string name) {
            if (!Equals(expected, actual)) {
                throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}");
            }
        }

        private static void AssertTrue(bool actual, string name) {
            if (!actual) {
                throw new InvalidOperationException($"{name}: expected true, actual false");
            }
        }
    }
}
