using System.Numerics;
using Zenseless.Geometry;

namespace GraphicsWar.ExtensionMethods
{
    /// <summary>
    /// Implements a orbiting transformation
    /// </summary>
    /// <seealso cref="ITransformation" />
    public class Position : NotifyPropertyChanged, ITransformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Orbit"/> class.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="azimuth">The azimuth or heading.</param>
        /// <param name="elevation">The elevation or tilt.</param>
        public Position(Vector3 location, float azimuth = 0f, float elevation = 0f)
        {
            _cachedMatrixView = new CachedCalculatedValue<Matrix4x4>(CalcViewMatrix);
            PropertyChanged += (s, a) => _cachedMatrixView.Invalidate();
            Azimuth = azimuth;
            Elevation = elevation;
            _location = location;

        }

        /// <summary>
        /// Gets or sets the azimuth or heading.
        /// </summary>
        /// <value>
        /// The azimuth.
        /// </value>
        public float Azimuth
        {
            get => _azimuth;
            set
            {
                _azimuth = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the elevation or tilt.
        /// </summary>
        /// <value>
        /// The elevation.
        /// </value>
        public float Elevation
        {
            get => _elevation;
            set
            {
                _elevation = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the transformation matrix in row-major style.
        /// </summary>
        /// <value>
        /// The matrix.
        /// </value>
        public Matrix4x4 Matrix => _cachedMatrixView.Value;

        /// <summary>
        /// Gets or sets the target, the point the camera is looking at.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Vector3 Location
        {
            get => _location;
            set
            {
                _location = value;
                RaisePropertyChanged();
            }
        }

        private float _azimuth;
        private float _elevation;
        private Vector3 _location;
        private readonly CachedCalculatedValue<Matrix4x4> _cachedMatrixView;

        private Matrix4x4 CalcViewMatrix()
        {
            var mtxElevation = Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(Elevation));
            var mtxAzimut = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(Azimuth));
            var mtxLocation = Matrix4x4.CreateTranslation(-Location);
            return mtxLocation * mtxAzimut * mtxElevation;
        }
    }
}
