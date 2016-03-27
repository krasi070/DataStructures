namespace BunnyWars.Core
{
    using System;

    public class Bunny
    {
        private const int DefaultHealth = 100;
        private const int DefaultScore = 0;

        public Bunny(string name, int team, int roomId)
        {
            this.Name = name;
            this.Team = team;
            this.RoomId = roomId;
            this.Health = DefaultHealth;
            this.Score = DefaultScore;
        }

        public int RoomId { get; set; }

        public string Name { get; private set; }

        public int Health { get; set; }

        public int Score { get; set; }

        public int Team { get; private set; }
    }
}
