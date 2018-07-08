namespace Zenseless.Geometry
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Implements a Perspective transformation that allows incremental changes
    /// </summary>
    /// <seealso cref="ITransformation" />
    public class Ortographic : NotifyPropertyChanged, ITransformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Perspective"/> class.
        /// </summary>
        /// <param name="fieldOfViewY">The field-of-view in y-direction.</param>
        /// <param name="aspect">The aspect ratio.</param>
        /// <param name="nearClip">The near clip plane distance.</param>
        /// <param name="farClip">The far clip plane distance.</param>
        public Ortographic(float width, float height, float nearClip = 0.1f, float farClip = 100)
        {
            cachedMatrix = new CachedCalculatedValue<Matrix4x4>(CalculateProjectionMatrix);
            PropertyChanged += (s, a) => cachedMatrix.Invalidate();
            _width = width;
            _height = height;
            _nearClip = nearClip;
            _farClip = farClip;
        }


        /// <summary>
        /// Gets the transformation matrix in row-major style.
        /// </summary>
        /// <value>
        /// The matrix.
        /// </value>
        public Matrix4x4 Matrix => cachedMatrix.Value;

        /// <summary>
        /// Gets or sets the far clipping plane distance.
        /// </summary>
        /// <value>
        /// The far clipping plane distance.
        /// </value>
        public float Width
        {
            get => _width; set
            {
                Width = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the far clipping plane distance.
        /// </summary>
        /// <value>
        /// The far clipping plane distance.
        /// </value>
        public float Height
        {
            get => _height; set
            {
                _height = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the far clipping plane distance.
        /// </summary>
        /// <value>
        /// The far clipping plane distance.
        /// </value>
        public float FarClip
        {
            get => _farClip; set
            {
                _farClip = Math.Max(value, NearClip);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the near clipping plane distance.
        /// </summary>
        /// <value>
        /// The near clipping plane distance.
        /// </value>
        public float NearClip
        {
            get => _nearClip; set
            {
                _nearClip = Math.Max(value, float.Epsilon);
                RaisePropertyChanged();
            }
        }

        private float _width = 10;
        private float _height = 10;
        private float _farClip = 1f;
        private float _nearClip = 0.1f;
        private CachedCalculatedValue<Matrix4x4> cachedMatrix;

        private Matrix4x4 CalculateProjectionMatrix()
        {
            return Matrix4x4.CreateOrthographic(_width, _height, _nearClip, _farClip);
        }
    }
}
