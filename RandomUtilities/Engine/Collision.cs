using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomUtilities.MathUtil.Vectors;

namespace RandomUtilities.Engine
{
    public enum CollisionResultingBehavior { FullAbsorbing, HalfAbsorbing, FullReflectings }
    public class Collision
    {
        private Collider _baseCollider;
        private List<int> _baseColliderOutlineHitIndex;
        private Collider _comparedCollider;
        private List<int> _compColliderOutlineHitIndex;
        private bool _isHit;

        public Collider BaseCollider => _baseCollider;
        public List<int> BaseColliderOutlineHitIndex => new List<int>(_baseColliderOutlineHitIndex);
        public Collider ComparedCollider => _comparedCollider;
        public List<int> CompColliderOutlineHitIndex => new List<int>(_compColliderOutlineHitIndex);
        public bool IsHit => _isHit;

        public int HitCount => _baseColliderOutlineHitIndex.Count + _compColliderOutlineHitIndex.Count;

        public CollisionResultingBehavior Behavior  
        {
            get
            {
                if (BaseCollider.CollisionBehavior == CollisionBehaviors.Absorbing && ComparedCollider.CollisionBehavior == CollisionBehaviors.Absorbing)
                    return CollisionResultingBehavior.FullAbsorbing;
                if (BaseCollider.CollisionBehavior == CollisionBehaviors.Reflecting && ComparedCollider.CollisionBehavior == CollisionBehaviors.Reflecting)
                    return CollisionResultingBehavior.FullReflectings;

                return CollisionResultingBehavior.HalfAbsorbing;
            }
        }

        public Collision() : this(null, null, new List<int>(), new List<int>(), false)
        {
        }

        public Collision(Collider baseCollider, Collider comparedCollider, List<int> baseColliderOutlineHitIndex, List<int> compColliderOutlineHitIndex, bool isHit)
        {
            this._baseCollider = baseCollider;
            this._comparedCollider = comparedCollider;
            this._compColliderOutlineHitIndex = compColliderOutlineHitIndex;
            this._baseColliderOutlineHitIndex = baseColliderOutlineHitIndex;
            this._isHit = isHit;
        }
    }
}
