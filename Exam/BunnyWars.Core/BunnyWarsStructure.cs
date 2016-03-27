namespace BunnyWars.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Wintellect.PowerCollections;

    public class BunnyWarsStructure : IBunnyWarsStructure
    {
        private Dictionary<int, List<Bunny>> bunniesByRoomId = new Dictionary<int, List<Bunny>>();
        private OrderedSet<Bunny> orderedBunnies = new OrderedSet<Bunny>(bunnySuffixComparer); 
        private Dictionary<string, Bunny> bunnyByName = new Dictionary<string, Bunny>();
        private Dictionary<int, OrderedSet<Bunny>> bunniesByTeam = new Dictionary<int, OrderedSet<Bunny>>();
        private OrderedSet<int> roomIds = new OrderedSet<int>();

        private static Comparer<Bunny> bunnySuffixComparer = Comparer<Bunny>.Create((b1, b2) =>
        {
            string reversedFirstName = ReverseName(b1.Name);
            string reversedSecondName = ReverseName(b2.Name);
        
            return string.CompareOrdinal(reversedFirstName, reversedSecondName);
        }); 

        public int BunnyCount
        {
            get
            {
                return this.bunnyByName.Count;
            }
        }

        public int RoomCount
        {
            get
            {
                return this.bunniesByRoomId.Count;
            }
        }

        public void AddRoom(int roomId)
        {
            if (this.bunniesByRoomId.ContainsKey(roomId))
            {
                throw new ArgumentException("Room ID already exists.");
            }

            this.bunniesByRoomId.Add(roomId, new List<Bunny>());
            this.roomIds.Add(roomId);
        }

        public void AddBunny(string name, int team, int roomId)
        {
            if (this.bunnyByName.ContainsKey(name))
            {
                throw new ArgumentException(string.Format("A bunny with the name {0} already exists.", name));
            }

            if (!this.bunniesByRoomId.ContainsKey(roomId))
            {
                throw new ArgumentException("The givem ID does not exist.");
            }

            if (team < 0 || team > 4)
            {
                throw new IndexOutOfRangeException("Team can only be an integer from 0 to 4.");
            }

            var newBunny = new Bunny(name, team, roomId);

            this.bunniesByRoomId[roomId].Add(newBunny);
            this.bunnyByName.Add(name, newBunny);
            this.orderedBunnies.Add(newBunny);
            if (!this.bunniesByTeam.ContainsKey(team))
            {
                this.bunniesByTeam[team] = new OrderedSet<Bunny>((b1, b2) => b2.Name.CompareTo(b1.Name));
            }

            this.bunniesByTeam[team].Add(newBunny);
        }

        public void Remove(int roomId)
        {
            if (!this.bunniesByRoomId.ContainsKey(roomId))
            {
                throw new ArgumentException("The given ID does not exist.");
            }

            var removedBunnies = this.bunniesByRoomId[roomId];
            this.bunniesByRoomId.Remove(roomId);
            this.roomIds.Remove(roomId);
            foreach (var removedBunny in removedBunnies)
            {
                this.orderedBunnies.Remove(removedBunny);
                this.bunnyByName.Remove(removedBunny.Name);
                this.bunniesByTeam[removedBunny.Team].Remove(removedBunny);
            }
        }

        public void Next(string bunnyName)
        {
            if (!this.bunnyByName.ContainsKey(bunnyName))
            {
                throw new ArgumentException("A bunny with the given name does not exist.");
            }

            var bunnyToMove = this.bunnyByName[bunnyName];
            int indexOfCurrRoom = this.roomIds.IndexOf(bunnyToMove.RoomId);
            if (indexOfCurrRoom != this.roomIds.Count - 1)
            {
                bunnyToMove.RoomId = this.roomIds.RangeFrom(bunnyToMove.RoomId, false).First();
            }
            else
            {
                bunnyToMove.RoomId = this.roomIds.First();
            }
        }

        public void Previous(string bunnyName)
        {
            if (!this.bunnyByName.ContainsKey(bunnyName))
            {
                throw new ArgumentException("A bunny with the given name does not exist.");
            }

            var bunnyToMove = this.bunnyByName[bunnyName];
            int indexOfCurrRoom = this.roomIds.IndexOf(bunnyToMove.RoomId);
            if (indexOfCurrRoom != 0)
            {
                bunnyToMove.RoomId = this.roomIds.RangeTo(bunnyToMove.RoomId, false).Reversed().First();
            }
            else
            {
                bunnyToMove.RoomId = this.roomIds.Reversed().First();
            }
        }

        public void Detonate(string bunnyName)
        {
            if (!this.bunnyByName.ContainsKey(bunnyName))
            {
                throw new ArgumentException("A bunny with the given name does not exist.");
            }

            var detonatedBunny = this.bunnyByName[bunnyName];

            var otherTeamBunnies = this.bunniesByRoomId[detonatedBunny.RoomId].ToList();
            foreach (var bunny in otherTeamBunnies)
            {
                if (bunny.Team != detonatedBunny.Team)
                {
                    bunny.Health -= 30;
                    if (bunny.Health <= 0)
                    {
                        this.RemoveBunny(bunny.Name);
                        detonatedBunny.Score++;
                    }
                }
            }
        }

        public IEnumerable<Bunny> ListBunniesByTeam(int team)
        {
            if (team < 0 || team > 4)
            {
                throw new IndexOutOfRangeException();
            }

            return this.bunniesByTeam[team];
        }

        public IEnumerable<Bunny> ListBunniesBySuffix(string suffix)
        {
            var low = new Bunny(suffix, 0, 0);
            var upper = new Bunny(char.MaxValue + suffix, 0, 0);

            return this.orderedBunnies.Range(low, true, upper, true);
        }

        private void RemoveBunny(string bunnyName)
        {
            var bunnyToRemove = this.bunnyByName[bunnyName];

            this.bunnyByName.Remove(bunnyName);
            this.orderedBunnies.Remove(bunnyToRemove);
            this.bunniesByRoomId[bunnyToRemove.RoomId].Remove(bunnyToRemove);
            this.bunniesByTeam[bunnyToRemove.Team].Remove(bunnyToRemove);
        }

        private static string ReverseName(string name)
        {
            return new string(name.Reverse().ToArray());
        }
    }
}
