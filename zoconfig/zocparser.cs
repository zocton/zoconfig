using System.Diagnostics.CodeAnalysis;

namespace zoconfig
{
    public static class ZoconfigExtensions
    {
        public static T? As<T>(this object obj)
        {
            if (obj is null) return default(T);

            string value = obj.ToString()!;
            if (typeof(T) == typeof(bool))
                return (T)Convert.ChangeType(bool.Parse(value), typeof(T));
            if (typeof(T) == typeof(byte))
                return (T)Convert.ChangeType(byte.Parse(value), typeof(T));
            if (typeof(T) == typeof(sbyte))
                return (T)Convert.ChangeType(sbyte.Parse(value), typeof(T));
            if (typeof(T) == typeof(char))
                return (T)Convert.ChangeType(char.Parse(value), typeof(T));
            if (typeof(T) == typeof(decimal))
                return (T)Convert.ChangeType(decimal.Parse(value), typeof(T));
            if (typeof(T) == typeof(double))
                return (T)Convert.ChangeType(double.Parse(value), typeof(T));
            if (typeof(T) == typeof(float))
                return (T)Convert.ChangeType(float.Parse(value), typeof(T));
            if (typeof(T) == typeof(int))
                return (T)Convert.ChangeType(int.Parse(value), typeof(T));
            if (typeof(T) == typeof(uint))
                return (T)Convert.ChangeType(uint.Parse(value), typeof(T));
            if (typeof(T) == typeof(nint))
                return (T)Convert.ChangeType(nint.Parse(value), typeof(T));
            if (typeof(T) == typeof(nuint))
                return (T)Convert.ChangeType(nuint.Parse(value), typeof(T));
            if (typeof(T) == typeof(long))
                return (T)Convert.ChangeType(long.Parse(value), typeof(T));
            if (typeof(T) == typeof(ulong))
                return (T)Convert.ChangeType(ulong.Parse(value), typeof(T));
            if (typeof(T) == typeof(short))
                return (T)Convert.ChangeType(short.Parse(value), typeof(T));
            if (typeof(T) == typeof(ushort))
                return (T)Convert.ChangeType(byte.Parse(value), typeof(T));

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }

    public class ZoconfigException : Exception
    {
        public ZoconfigException(string? message) : base(message)
        {
        }
    }

    public struct Zobject
    {
        public Dictionary<string, object> Data { get; set; }

        public Zobject()
        {
            Data = new Dictionary<string, object>();
        }

        public object this[string name] => Data[name];
    }

    public class Zocstream
    {
        public Dictionary<string, Zobject>? Binds { get; set; }
        public Zobject? this[string name] => Binds?[name];
    }

    public class Zocparser
    {
        const char comment = '#';
        const char objectOpen = '[';
        const char objectClose = ']';
        const char objectBind = ':';
        const char typeOpen = '(';
        const char typeClose = ')';
        const char scopeOpen = '{';
        const char scopeClose = '}';

        Dictionary<string, Type> dataTypes = new Dictionary<string, Type>
        {
            { "bool",   typeof(bool) },
            { "byte",   typeof(byte) },
            { "sbyte",  typeof(sbyte) },
            { "char",   typeof(char) },
            { "decimal",typeof(decimal) },
            { "double", typeof(double) },
            { "float",  typeof(float) },
            { "int",    typeof(int) },
            { "uint",   typeof(uint) },
            { "nint",   typeof(nint) },
            { "nuint",  typeof(nuint) },
            { "long",   typeof(long) },
            { "ulong",  typeof(ulong) },
            { "short",  typeof(short) },
            { "ushort", typeof(ushort) },
            { "string", typeof(string) },
            { "object", typeof(object) }
        };

        Dictionary<string, Zobject> binds = new Dictionary<string, Zobject>();
        string? _fileData;
        public Zocstream? Stream { get; private set; }
        public Zobject? this[string name] => Stream?[name];

        private bool ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (!File.Exists(path)) return false;

            return true;
        }

        private async Task<string?> LoadAsync(string path)
        {
            if (!ValidatePath(path)) return null;

            using (var stream = new StreamReader(path))
            {
                _fileData = await stream.ReadToEndAsync();
            }

            return _fileData;
        }

        private string? Load(string path)
        {
            if (!ValidatePath(path)) return null;

            using (var stream = new StreamReader(path))
            {
                _fileData = stream.ReadToEnd();
            }

            return _fileData;
        }

        /// <summary> 
        /// Asynchronously retrieves zoconfig file data.
        /// </summary>
        /// <param name="path">The path to your *.zc file.</param>
        /// <exception cref="ZoconfigException"/>
        public async void GetDataAsync(string path)
        {
            string? data = await LoadAsync(path);

            if (data is null)
                throw new ZoconfigException($"Could not load data {path}");

            Stream = Parse(data);
        }

        /// <summary> 
        /// Synchronously retrieves zoconfig file data.
        /// </summary>
        /// <param name="path">The path to your *.zc file.</param>
        /// <exception cref="ZoconfigException"/>
        public void GetData(string path)
        {
            string? data = Load(path);

            if (data is null)
                throw new ZoconfigException($"Could not load data {path}");

            Stream = Parse(data);
        }

        private string GetObjectName(string objectText)
        {
            objectText = objectText.Trim();

            if (objectText[0] == objectOpen && objectText[objectText.Length - 1] == objectClose)
                return objectText.Substring(1, objectText.Length - 2);
            else
                throw new ZoconfigException($"{objectText} does not have proper syntax.");
        }

        private object GetObjectData(string objectText)
        {
            objectText = objectText.Trim();
            string? typeName = null;

            if (objectText[0] == typeOpen)
            {
                int closure = objectText.IndexOf(typeClose);

                if (closure == -1)
                    throw new ZoconfigException($"{objectText} does not provide closure for type specifier.");

                typeName = objectText.Substring(1, closure - 1);
            }

            int open = objectText.IndexOf(objectOpen);
            int close = objectText.IndexOf(objectClose);

            if (open == -1 || close == -1)
                throw new ZoconfigException($"{objectText} syntax malformed.");

            open++;
            string value = objectText.Substring(open, close - open);

            return value;
        }

        private Zocstream Parse(string data)
        {
            Zocstream stream = new Zocstream();

            data.ReplaceLineEndings("\n");
            string[] lines = data.Split('\n');
            string? currentObjectName = null;
            bool isInScope = false;

            foreach (string line in lines)
            {
                // Is this line cared for
                if (string.IsNullOrWhiteSpace(line)) continue;

                string trimmed = line.Trim();

                // Ignore comments
                if (trimmed[0] == comment) continue;

                // Check for the start of object scope
                if (trimmed.Length == 1 && currentObjectName is not null && trimmed[0] == scopeOpen)
                {
                    isInScope = true;
                    continue;
                }

                // If we are not in scope then an object definition is expected
                if (!isInScope)
                {
                    if (currentObjectName is not null)
                        throw new ZoconfigException("Multiple objects declared in a row.");

                    currentObjectName = GetObjectName(line);
                    binds.Add(currentObjectName, new Zobject());
                    continue;
                }
                // Otherwise, check if scope is closing
                else
                {
                    if (trimmed.Length == 1 && trimmed[0] == scopeClose)
                    {
                        isInScope = false;
                        currentObjectName = null;
                        continue;
                    }
                }

                // If scope isn't being declared or closed we are parsing object data
                string[] info = line.Split(objectBind);

                // Objects can only have a name and value pair
                if (info.Length != 2)
                    throw new ZoconfigException($"{line} contains more than one binding character ':'");

                // Name
                info[0] = info[0].Trim();

                // Value
                info[1] = info[1].Trim();

                if (currentObjectName is null)
                    throw new ZoconfigException("Object is unreachable.");

                binds[currentObjectName].Data.Add(GetObjectName(info[0]), GetObjectData(info[1]));
            }

            stream.Binds = binds;
            return stream;
        }
    }
}