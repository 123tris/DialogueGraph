using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class SerializableCallbackBase<TReturn> : SerializableCallbackBase {
	public InvokableCallbackBase<TReturn> func;

	public override void ClearCache() {
		base.ClearCache();
		func = null;
	}

	protected InvokableCallbackBase<TReturn> GetPersistentMethod() {
		Type[] types = new Type[ArgRealTypes.Length + 1];
		Array.Copy(ArgRealTypes, types, ArgRealTypes.Length);
		types[types.Length - 1] = typeof(TReturn);

		Type genericType = null;
		switch (types.Length) {
			case 1:
				genericType = typeof(InvokableCallback<>).MakeGenericType(types);
				break;
			case 2:
				genericType = typeof(InvokableCallback<,>).MakeGenericType(types);
				break;
			case 3:
				genericType = typeof(InvokableCallback<, ,>).MakeGenericType(types);
				break;
			case 4:
				genericType = typeof(InvokableCallback<, , ,>).MakeGenericType(types);
				break;
			case 5:
				genericType = typeof(InvokableCallback<, , , ,>).MakeGenericType(types);
				break;
			default:
				throw new ArgumentException(types.Length + "args");
		}
		return Activator.CreateInstance(genericType, target, methodName) as InvokableCallbackBase<TReturn>;
	}
}

/// <summary> An inspector-friendly serializable function </summary>
[Serializable]
public abstract class SerializableCallbackBase : ISerializationCallbackReceiver {

	/// <summary> Target object </summary>
	public Object target { get => _target;
        set { _target = value; ClearCache(); } }
	/// <summary> Target method name </summary>
	public string methodName { get => _methodName;
        set { _methodName = value; ClearCache(); } }
	public object[] Args { get { return args ??= _args.Select(x => x.GetValue()).ToArray(); } }
	public object[] args;
	public Type[] ArgTypes { get { return argTypes ??= _args.Select(x => Arg.RealType(x.argType)).ToArray(); } }
	public Type[] argTypes;
	public Type[] ArgRealTypes { get { return argRealTypes ??= _args.Select(x => Type.GetType(x._typeName)).ToArray(); } }
	public Type[] argRealTypes;
	public bool dynamic { get => _dynamic;
        set { _dynamic = value; ClearCache(); } }

	[SerializeField] protected Object _target;
	[SerializeField] protected string _methodName;
	[SerializeField] protected Arg[] _args;
	[SerializeField] protected bool _dynamic;
#pragma warning disable 0414
	[SerializeField] private string _typeName;
#pragma warning restore 0414

	[SerializeField] private bool dirty;

#if UNITY_EDITOR
	protected SerializableCallbackBase() {
		_typeName = GetType().AssemblyQualifiedName;
	}
#endif

	public virtual void ClearCache() {
		argTypes = null;
		args = null;
	}

	public void SetMethod(Object target, string methodName, bool dynamic, params Arg[] args) {
		_target = target;
		_methodName = methodName;
		_dynamic = dynamic;
		_args = args;
		ClearCache();
	}

	protected abstract void Cache();

	public void OnBeforeSerialize() {
#if UNITY_EDITOR
		if (dirty) { ClearCache(); dirty = false; }
#endif
	}

	public void OnAfterDeserialize() {
#if UNITY_EDITOR
		_typeName = GetType().AssemblyQualifiedName;
#endif
	}
}

[Serializable]
public struct Arg {
	public enum ArgType { Unsupported, Bool, Int, Float, String, Object }
	public bool boolValue;
	public int intValue;
	public float floatValue;
	public string stringValue;
	public Object objectValue;
	public ArgType argType;
	public string _typeName;

	public object GetValue() {
		return GetValue(argType);
	}

	public object GetValue(ArgType type)
    {
        return type switch
        {
            ArgType.Bool => boolValue,
            ArgType.Int => intValue,
            ArgType.Float => floatValue,
            ArgType.String => stringValue,
            ArgType.Object => objectValue,
            _ => null
        };
    }

	public static Type RealType(ArgType type)
    {
        return type switch
        {
            ArgType.Bool => typeof(bool),
            ArgType.Int => typeof(int),
            ArgType.Float => typeof(float),
            ArgType.String => typeof(string),
            ArgType.Object => typeof(Object),
            _ => null
        };
    }

	public static ArgType FromRealType(Type type)
    {
        if (type == typeof(bool)) return ArgType.Bool;
        if (type == typeof(int)) return ArgType.Int;
        if (type == typeof(float)) return ArgType.Float;
        if (type == typeof(string)) return ArgType.String;
        if (typeof(Object).IsAssignableFrom(type)) return ArgType.Object;
        return ArgType.Unsupported;
    }

	public static bool IsSupported(Type type) {
		return FromRealType(type) != ArgType.Unsupported;
	}
}