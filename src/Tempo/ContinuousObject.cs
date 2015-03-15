using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo
{
    /// <summary>
    /// Provides a way for a continuous block to be constructed imperatively. ContinuousObject encapsulates a continuous block.
    /// Inheritors should call InitScope in the constructor. This will activate the inner block. The block is ended when the
    /// ContinuousObject's reference count reaches zero. Ineritors should implement the WhileAlive method, which is active when
    /// the inner scope is active. Inheritors may override Destroy to perform cleanup when the object is destroyed. If Destroy is
    /// overridden, then base.Destroy must be called to end the inner continuous block.
    /// </summary>
    public abstract class ContinuousObject : RefCountedSafe
    {
        private LifetimeSource objectLifetimeSrc = new LifetimeSource();


        /// <summary>
        /// Constructs a new ContinuousObject, and initializes the inner continuous block. Inheritors should call InitScope at
        /// some point in the constructor.
        /// </summary>
        public ContinuousObject()
        {
        }

        /// <summary>
        /// Constructs the inner continuous scope. Inheritors should call this in the constructor.
        /// </summary>
        protected void InitScope()
        {
            CurrentThread.ConstructScope(CurrentThread.AnyCurrentScope(), objectLifetimeSrc.Lifetime, WhileAlive);
        }

        /// <summary>
        /// This method is the constructor for the inner continuous block. The block is ended when the ContinuousObject's reference
        /// count reaches zero.
        /// </summary>
        protected abstract void WhileAlive();

        protected override void Destroy()
        {
            objectLifetimeSrc.EndLifetime();
        }
    }
}
