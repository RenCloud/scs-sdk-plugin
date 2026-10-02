namespace SCSSdkClient {
    /// <summary>
    ///     Type of the currently active job.
    /// </summary>
    public enum JobType {
        /// <summary>
        ///     No active job.
        /// </summary>
        None = 0,

        /// <summary>
        ///     An ordinary freight job.
        /// </summary>
        Freight = 1,

        /// <summary>
        ///     A car job.
        /// </summary>
        Car = 2
    }
}
