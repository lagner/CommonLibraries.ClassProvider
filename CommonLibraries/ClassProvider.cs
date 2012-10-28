using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;



namespace CommonLibraries
{

    public class ParentClass : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Property changed
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion



        #region Error changed

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            return new string[] { "", };
        }

        public bool HasErrors
        {
            get { return false; }
        }

        #endregion
    }

    public class ClassProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">a collection of property name/property type keyvaluespairs.</param>
        /// <param name="name">name of class</param>
        /// <param name="suppressExceptions">can class returns an exeption, or null(default)</param>
        public ClassProvider(Dictionary<string, Type> properties, string name, bool suppressExceptions = true)
        {
            if (properties == null ||
                String.IsNullOrWhiteSpace(name))
                throw new Exception("Wrong parameters");

            this.properties = properties;
            this.className = name;
            this.suppressExceptions = suppressExceptions;
        }

        /// <summary>
        /// Create class with name above and numbers of public properties
        /// wich support Inotifypropertychanged interface
        /// </summary>
        /// <returns>returns type of the created class or null if an exception was throwed</returns>
        public Type CreateClass()
        {
            string assemblyName = "AddedAssembly";

            try
            {
                AppDomain domain = AppDomain.CurrentDomain;
                AssemblyName newAssembly = new AssemblyName(assemblyName);
                AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(newAssembly, AssemblyBuilderAccess.Run);

                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(newAssembly.Name);

                TypeBuilder newType = moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(ParentClass));

                // for notify property changed interface
                MethodInfo notify = (typeof(ParentClass)).GetMethod("Notify");
                // nessesary arguments for public property
                MethodAttributes atrs = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                foreach (var property in properties)
                {
                    // create private field
                    FieldBuilder field = newType.DefineField("_" + property.Key, property.Value, FieldAttributes.Private);
                    // create public property
                    PropertyBuilder prop = newType.DefineProperty(property.Key, PropertyAttributes.HasDefault, property.Value, null);

                    // Create get method for the public property
                    MethodBuilder getMethod = newType.DefineMethod("get_" + property.Key, atrs, property.Value, Type.EmptyTypes);

                    ILGenerator il = getMethod.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Ret);


                    // Create set method for the public property
                    MethodBuilder setMethod = newType.DefineMethod("set_" + property.Key, atrs, null, new Type[] { property.Value, });

                    il = setMethod.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Ldarg_0);   // 
                    il.Emit(OpCodes.Ldstr, property.Key); //
                    il.EmitCall(OpCodes.Call, notify, new Type[] { typeof(string), }); // notify method invoke
                    il.Emit(OpCodes.Ret);

                    //
                    prop.SetGetMethod(getMethod);
                    prop.SetSetMethod(setMethod);
                }

                return newType.CreateType();
            }
            catch (Exception ex)
            {
                if (suppressExceptions)
                    return null;
                else
                    throw ex;
            }
        }

        private Dictionary<string, Type> properties;
        private string className;
        private bool suppressExceptions = true;
    }
}

