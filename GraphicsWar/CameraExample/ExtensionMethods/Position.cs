using System;
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
        /// <param name="distance">The distance to the target.</param>
        /// <param name="azimuth">The azimuth or heading.</param>
        /// <param name="elevation">The elevation or tilt.</param>
        public Position(Vector3 location, float azimuth = 0f, float elevation = 0f)
        {
            cachedMatrixView = new CachedCalculatedValue<Matrix4x4>(CalcViewMatrix);
            PropertyChanged += (s, a) => cachedMatrixView.Invalidate();
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
        public Matrix4x4 Matrix => cachedMatrixView.Value;

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

        /// <summary>
        /// Calculates the camera position.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArithmeticException">Could not invert matrix</exception>
        public Vector3 CalcPosition()
        {
            if (!Matrix4x4.Invert(Matrix, out Matrix4x4 inverse)) throw new ArithmeticException("Could not invert matrix");
            return inverse.Translation;
        }

        private float _azimuth = 0f;
        private float _elevation = 0f;
        private Vector3 _location = Vector3.Zero;
        private readonly CachedCalculatedValue<Matrix4x4> cachedMatrixView;

        private Matrix4x4 CalcViewMatrix()
        {
            var mtxElevation = Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(Elevation));
            var mtxAzimut = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(Azimuth));
            var mtxLocation = Matrix4x4.CreateTranslation(-Location);
            return mtxLocation * mtxAzimut * mtxElevation;
        }
    }
}
