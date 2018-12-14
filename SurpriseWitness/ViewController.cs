using System;

using AppKit;
using Foundation;

namespace SurpriseWitness
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // In a Xamarin.Mac.Sdk project, this will throw because the ref
            // assembly got bundled instead of the proper one from lib in the
            // System.Memory NuGet package.
            var x = System.Buffers.ReadOnlySequence<int>.Empty;
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
