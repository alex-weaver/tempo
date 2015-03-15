using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Scheduling
{
    /// <summary>
    /// This class wraps an update handler. This is needed in case a client registers the same handler for two
    /// separate updates - in this case each handler needs a unique identity.
    /// </summary>
    public class Updateable
    {
        private Action update;

        public Updateable(Action update)
        {
            if (update == null) throw new ArgumentNullException("update");
            this.update = update;
        }

        public void Update()
        {
            update();
        }
    }
}
