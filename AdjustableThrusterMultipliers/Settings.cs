namespace AdjustableThrusterMultipliers
{
    public class Settings
    {
        public ThrusterSettings GlobalThrusters { get; set; }
        public ThrusterSettings AtmosphericThruster { get; set; }
        public ThrusterSettings HydrogenThruster { get; set; }
        public ThrusterSettings IonThruster { get; set; }


        public string[] BlacklistedThrustSubtypes { get; set; } = new string[] { "ThrustSubtypeNameGoesHere" };

        public Settings()
        {
            GlobalThrusters = new ThrusterSettings();
            IonThruster = new ThrusterSettings();
            HydrogenThruster = new ThrusterSettings();
            AtmosphericThruster = new ThrusterSettings();
        }

        public Settings LoadSettings()
        {
            Settings settings = new Settings();

            if (settings.GlobalThrusters.MaxThrustMultiplier < 1)
            {
                settings.GlobalThrusters.MaxThrustMultiplier = 1;
            }

            if (settings.AtmosphericThruster.MaxThrustMultiplier < 1)
            {
                settings.AtmosphericThruster.MaxThrustMultiplier = 1;
            }

            if (settings.HydrogenThruster.MaxThrustMultiplier < 1)
            {
                settings.HydrogenThruster.MaxThrustMultiplier = 1;
            }

            if (settings.IonThruster.MaxThrustMultiplier < 1)
            {
                settings.IonThruster.MaxThrustMultiplier = 1;
            }

            return settings;
        }

        public void SaveSettings() {  }

        public class ThrusterSettings
        {
            public float MaxThrustMultiplier { get; set; } = 11f;
            public float FuelUsePerMultiplier { get; set; } = 2.5f;
        }
    }
}