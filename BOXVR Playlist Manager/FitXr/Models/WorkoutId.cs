using System;

namespace BoxVR_Playlist_Manager.FitXr.Models
{
    public struct WorkoutId : IEquatable<WorkoutId>
    {
        public string _id;

        public WorkoutId(string id) => this._id = id;

        public static bool operator ==(WorkoutId obj1, WorkoutId obj2)
        {
            if((ValueType)obj1 == (ValueType)obj2)
                return true;
            return (ValueType)obj1 != null && (ValueType)obj2 != null && obj1._id == obj2._id;
        }

        public static bool operator !=(WorkoutId obj1, WorkoutId obj2) => !(obj1 == obj2);

        public bool Equals(WorkoutId other)
        {
            if((ValueType)other == null)
                return false;
            return (ValueType)this == (ValueType)other || this._id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;
            if((ValueType)this == obj)
                return true;
            return obj.GetType() == this.GetType() && this.Equals((WorkoutId)obj);
        }

        public override int GetHashCode() => this._id.GetHashCode();
    }
}
