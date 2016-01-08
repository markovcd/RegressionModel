using System;
using System.Collections.Generic;
using System.Reflection;

namespace Simpro.Expr
{
    public class Using
    {
        internal Dictionary<string, Assembly> namespace_list = new Dictionary<string, Assembly>();
        public Using()
        {
            Add("System");
        }
        public void Add(string namespace_name)
        {
            if (namespace_list.ContainsKey(namespace_name))
                namespace_list[namespace_name] = null;
            else
                namespace_list.Add(namespace_name, null);
        }
        public void Add(string namespace_name, string assembly_name)
        {
            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assem.FullName.StartsWith(assembly_name, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (namespace_list.ContainsKey(namespace_name))
                        namespace_list[namespace_name] = assem;
                    else
                        namespace_list.Add(namespace_name, assem);
                    return;
                }
            }
            throw new ExprException("Can't find assembly.");
        }
    }
}
