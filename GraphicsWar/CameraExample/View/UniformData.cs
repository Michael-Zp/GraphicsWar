using System.Collections.Generic;
using System.Numerics;
using Zenseless.HLGL;

namespace GraphicsWar.View
{
    public class UniformData
    {
        private Dictionary<string, int> _intValues = new Dictionary<string, int>();
        private Dictionary<string, float> _floatValues = new Dictionary<string, float>();
        private Dictionary<string, Vector2> _vec2Values = new Dictionary<string, Vector2>();
        private Dictionary<string, Vector3> _vec3Values = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector4> _vec4Values = new Dictionary<string, Vector4>();
        private Dictionary<string, Matrix4x4> _mat4x4Values = new Dictionary<string, Matrix4x4>();

        public void SetValue(string key, int value)
        {
            if (_intValues.ContainsKey(key))
            {
                _intValues[key] = value;
            }
            else
            {
                _intValues.Add(key, value);
            }
        }

        public void SetValue(string key, float value)
        {
            if (_floatValues.ContainsKey(key))
            {
                _floatValues[key] = value;
            }
            else
            {
                _floatValues.Add(key, value);
            }
        }

        public void SetValue(string key, Vector2 value)
        {
            if (_vec2Values.ContainsKey(key))
            {
                _vec2Values[key] = value;
            }
            else
            {
                _vec2Values.Add(key, value);
            }
        }

        public void SetValue(string key, Vector3 value)
        {
            if (_vec3Values.ContainsKey(key))
            {
                _vec3Values[key] = value;
            }
            else
            {
                _vec3Values.Add(key, value);
            }
        }

        public void SetValue(string key, Vector4 value)
        {
            if (_vec4Values.ContainsKey(key))
            {
                _vec4Values[key] = value;
            }
            else
            {
                _vec4Values.Add(key, value);
            }
        }

        public void SetValue(string key, Matrix4x4 value)
        {
            if (_mat4x4Values.ContainsKey(key))
            {
                _mat4x4Values[key] = value;
            }
            else
            {
                _mat4x4Values.Add(key, value);
            }
        }

        public void ApplyUniforms(IShaderProgram shader)
        {
            foreach (string key in _intValues.Keys)
            {
                shader.Uniform(key, _intValues[key]);
            }

            foreach (string key in _floatValues.Keys)
            {
                shader.Uniform(key, _floatValues[key]);
            }

            foreach (string key in _vec2Values.Keys)
            {
                shader.Uniform(key, _vec2Values[key]);
            }

            foreach (string key in _vec3Values.Keys)
            {
                shader.Uniform(key, _vec3Values[key]);
            }

            foreach (string key in _vec4Values.Keys)
            {
                shader.Uniform(key, _vec4Values[key]);
            }

            foreach (string key in _mat4x4Values.Keys)
            {
                shader.Uniform(key, _mat4x4Values[key]);
            }
        }
    }
}
