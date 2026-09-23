using Waitingway.Timers;

var tests = new (string Name, Action Test)[]
{
    ("opens at even UTC hour", () => AssertState(
        new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc),
        OceanFishingTimerStatus.Open,
        TimeSpan.FromMinutes(15))),
    ("stays open through 14:59", () => AssertState(
        new DateTime(2026, 9, 23, 12, 14, 59, DateTimeKind.Utc),
        OceanFishingTimerStatus.Open,
        TimeSpan.FromSeconds(1))),
    ("closes at 15 minutes", () => AssertState(
        new DateTime(2026, 9, 23, 12, 15, 0, DateTimeKind.Utc),
        OceanFishingTimerStatus.Closed,
        TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(45)))),
    ("finds next window during odd hour", () => AssertState(
        new DateTime(2026, 9, 23, 13, 30, 0, DateTimeKind.Utc),
        OceanFishingTimerStatus.Closed,
        TimeSpan.FromMinutes(30))),
    ("rolls over midnight", () => AssertState(
        new DateTime(2026, 9, 23, 23, 59, 59, DateTimeKind.Utc),
        OceanFishingTimerStatus.Closed,
        TimeSpan.FromSeconds(1))),
    ("formats compact countdown", () => AssertEqual("04:05", OceanFishingSchedule.FormatRemaining(TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(5)), true))),
    ("formats compact long countdown", () => AssertEqual("1:04:05", OceanFishingSchedule.FormatRemaining(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(4)).Add(TimeSpan.FromSeconds(5)), true))),
    ("formats long countdown", () => AssertEqual("01:04:05", OceanFishingSchedule.FormatRemaining(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(4)).Add(TimeSpan.FromSeconds(5)), false))),
    ("aggregates airships and submersibles", TestDeployableAggregation),
    ("ignores idle deployables", TestIdleDeployables),
    ("orders pending deployables by return", TestDeployableOrdering),
    ("aggregates ready retainer ventures", TestRetainerAggregation),
    ("ignores invalid retainer ventures", TestInvalidRetainers),
    ("orders pending retainer ventures", TestRetainerOrdering),
};

foreach (var (name, test) in tests)
{
    test();
    Console.WriteLine($"PASS {name}");
}

static void AssertState(DateTime now, OceanFishingTimerStatus expectedStatus, TimeSpan expectedRemaining)
{
    var state = OceanFishingSchedule.GetState(now);
    AssertEqual(expectedStatus, state.Status);
    AssertEqual(expectedRemaining, state.Remaining);
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
}

static void TestDeployableAggregation()
{
    var now = DateTimeOffset.FromUnixTimeSeconds(1_000_000);
    var snapshots = new DeployableSnapshot[]
    {
        new("Alice", "Alpha", "Air One", DeployableType.Airship, 999_999),
        new("Alice", "Alpha", "Sub One", DeployableType.Submersible, 1_000_000),
        new("Bob", "Beta", "Sub Two", DeployableType.Submersible, 1_000_600),
    };

    var state = DeployableTracker.GetState(snapshots, now);
    AssertEqual(2, state.ReadyCount);
    AssertEqual(3, state.TotalCount);
    AssertEqual("Sub Two", state.NextReturn!.Value.Name);
}

static void TestIdleDeployables()
{
    var state = DeployableTracker.GetState(
        [new("Alice", "Alpha", "Idle", DeployableType.Airship, 0)],
        DateTimeOffset.FromUnixTimeSeconds(1_000_000));

    AssertEqual(0, state.TotalCount);
    AssertEqual(0, state.ReadyCount);
    AssertEqual<DeployableSnapshot?>(null, state.NextReturn);
}

static void TestDeployableOrdering()
{
    var now = DateTimeOffset.FromUnixTimeSeconds(1_000_000);
    var state = DeployableTracker.GetState(
        [
            new("Alice", "Alpha", "Later", DeployableType.Airship, 1_000_100),
            new("Alice", "Alpha", "Soon", DeployableType.Submersible, 1_000_001),
        ],
        now);

    AssertEqual("Soon", state.NextReturn!.Value.Name);
    AssertEqual(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(state.NextReturn.Value.ReturnTimeUnixSeconds - now.ToUnixTimeSeconds()));
}

static void TestRetainerAggregation()
{
    var now = DateTimeOffset.FromUnixTimeSeconds(2_000_000);
    var state = RetainerTracker.GetState(
        [
            new("Alice", "Alpha", "Ready", 2_000_000),
            new("Bob", "Beta", "Pending", 2_000_600),
        ],
        now);

    AssertEqual(1, state.ReadyCount);
    AssertEqual(2, state.TotalCount);
    AssertEqual("Pending", state.NextReturn!.Value.Name);
}

static void TestInvalidRetainers()
{
    var state = RetainerTracker.GetState(
        [new("Alice", "Alpha", "Invalid", 0)],
        DateTimeOffset.FromUnixTimeSeconds(2_000_000));

    AssertEqual(0, state.TotalCount);
    AssertEqual<RetainerSnapshot?>(null, state.NextReturn);
}

static void TestRetainerOrdering()
{
    var now = DateTimeOffset.FromUnixTimeSeconds(2_000_000);
    var state = RetainerTracker.GetState(
        [
            new("Alice", "Alpha", "Later", 2_000_100),
            new("Alice", "Alpha", "Soon", 2_000_001),
        ],
        now);

    AssertEqual("Soon", state.NextReturn!.Value.Name);
    AssertEqual("00:01", RetainerTracker.FormatRemaining(state.NextReturn.Value.VentureEndsAtUnixSeconds, now, false));
}
