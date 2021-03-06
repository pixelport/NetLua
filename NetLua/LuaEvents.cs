﻿/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2013 Francesco Bertolaccini
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLua
{
    static class LuaEvents
    {
        // This is all based on the Lua 5.2 manual
        // http://www.lua.org/manual/5.2/manual.html#2.4

        public static LuaObject rawget(LuaObject table, LuaObject index)
        {
            if (table.IsTable)
            {
                LuaObject obj;
                if (table.AsTable().TryGetValue(index, out obj))
                    return obj;
                else
                    return LuaObject.Nil;
            }
            else
                throw new LuaException("Invalid operation");
        }

        public static void rawset(LuaObject table, LuaObject key, LuaObject value)
        {
            if (table.IsTable)
            {
                var t = table.AsTable();
                if (t.ContainsKey(key))
                    t[key] = value;
                else
                    t.Add(key, value);
            }
            else
                throw new LuaException("Invalid operation");
        }

        static LuaObject getBinhandler(LuaObject a, LuaObject b, string f)
        {
            var f1 = getMetamethod(a, f);
            var f2 = getMetamethod(b, f);

            return f1 | f2;
        }

        static LuaObject getEqualhandler(LuaObject a, LuaObject b)
        {
            if ((a.Type != b.Type) || (a.IsTable && b.IsUserData))
                return LuaObject.Nil;
            var mm1 = getMetamethod(a, "__eq");
            var mm2 = getMetamethod(b, "__eq");
            if (mm1 == mm2)
                return mm1;
            else
                return LuaObject.Nil;
        }

        internal static LuaObject toNumber(LuaObject obj)
        {
            if (obj.IsNumber)
                return LuaObject.FromNumber(obj.AsNumber());
            else if (obj.IsString)
            {
                double d;
                if (double.TryParse(obj.AsString(), out d))
                    return LuaObject.FromNumber(d);
                else
                    return LuaObject.Nil;
            }
            else
                return LuaObject.Nil;
        }

        internal static LuaObject getMetamethod(LuaObject obj, string e)
        {
            if (obj.Metatable == null || obj.Metatable.IsNil)
                return LuaObject.Nil;
            else
                return obj.Metatable[e];
        }

        internal static LuaObject add_event(LuaObject op1, LuaObject op2)
        {
            var a = toNumber(op1);
            var b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
                return LuaObject.FromNumber(a.AsNumber() + b.AsNumber());
            else
            {
                var handler = getBinhandler(op1, op2, "__add");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject sub_event(LuaObject op1, LuaObject op2)
        {
            var a = toNumber(op1);
            var b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
                return LuaObject.FromNumber(a.AsNumber() - b.AsNumber());
            else
            {
                var handler = getBinhandler(op1, op2, "__sub");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject mul_event(LuaObject op1, LuaObject op2)
        {
            var a = toNumber(op1);
            var b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
                return LuaObject.FromNumber(a.AsNumber() * b.AsNumber());
            else
            {
                var handler = getBinhandler(op1, op2, "__mul");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject div_event(LuaObject op1, LuaObject op2)
        {
            var a = toNumber(op1);
            var b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
                return LuaObject.FromNumber(a.AsNumber() / b.AsNumber());
            else
            {
                var handler = getBinhandler(op1, op2, "__div");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject mod_event(LuaObject op1, LuaObject op2)
        {
            var a = toNumber(op1);
            var b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
                return LuaObject.FromNumber(a.AsNumber() - Math.Floor(a.AsNumber() / b.AsNumber()) * b.AsNumber());
            else
            {
                var handler = getBinhandler(op1, op2, "__mod");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject pow_event(LuaObject op1, LuaObject op2)
        {
            var a = toNumber(op1);
            var b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
                return LuaObject.FromNumber(Math.Pow(a.AsNumber(), b.AsNumber()));
            else
            {
                var handler = getBinhandler(op1, op2, "__pow");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject unm_event(LuaObject op)
        {
            var a = toNumber(op);

            if (!a.IsNil)
            {
                double o1 = a.AsNumber();
                return LuaObject.FromNumber(-o1);
            }
            else
            {
                var handler = getMetamethod(op, "__unm");
                if (!handler.IsNil)
                    return handler.Call(op)[0];
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject index_event(LuaObject table, LuaObject key)
        {
            LuaObject handler;

            if (table.IsTable)
            {
                var v = rawget(table, key);
                if (!v.IsNil)
                    return v;
                else
                {
                    handler = getMetamethod(table, "__index");
                    if (handler.IsNil)
                        return LuaObject.Nil;
                }
            }
            else
            {
                handler = getMetamethod(table, "__index");
                if (handler.IsNil)
                    throw new LuaException("Invalid argument");
            }

            if (handler.IsFunction)
                return handler.AsFunction()(new LuaObject[]{table, key})[0];
            else if (!handler.IsNil)
                return handler[key];
            else
                return LuaObject.Nil;
        }

        internal static LuaObject newindex_event(LuaObject table, LuaObject key, LuaObject value)
        {
            LuaObject handler;
            if (table.IsTable)
            {
                var v = rawget(table, key);
                if (!v.IsNil)
                {
                    rawset(table, key, value);
                    return LuaObject.Nil;
                }
                handler = getMetamethod(table, "__newindex");
                if (handler.IsNil)
                {
                    rawset(table, key, value);
                    return LuaObject.Nil;
                }
            }
            else
            {
                handler = getMetamethod(table, "__newindex");
                if (handler.IsNil)
                    throw new LuaException("Invalid op");
            }

            if (handler.IsFunction)
                handler.AsFunction()(new LuaObject[]{table, key, value});
            else
                handler[key] = value;
            return LuaObject.Nil;
        }

        internal static LuaArguments call_event(LuaObject func, LuaArguments args)
        {
            if (func.IsFunction)
                return func.AsFunction()(args);
            else
            {
                var handler = getMetamethod(func, "__call");
                if (handler.IsFunction)
                {
                    var argslist = new List<LuaObject>();
                    argslist.Add(func); argslist.AddRange(args);
                    return handler.AsFunction()(argslist.ToArray());
                }
                else
                    throw new LuaException("Cannot call nonfunction");
            }
        }

        internal static LuaObject len_event(LuaObject op)
        {
            if (op.IsString)
                return op.AsString().Length;
            else
            {
                var handler = getMetamethod(op, "__len");
                if (!handler.IsNil)
                    return handler.Call(op)[0];
                else if (op.IsTable)
                    return op.AsTable().Count;
                else
                    throw new LuaException("Invalid op");
            }
        }

        internal static LuaObject concat_event(LuaObject op1, LuaObject op2)
        {
            if ((op1.IsString || op1.IsNumber) && (op2.IsString || op2.IsNumber))
                return op1.ToString() + op2.ToString();
            else
            {
                var handler = getBinhandler(op1, op2, "__concat");
                if (!handler.IsNil)
                    return handler.Call(op1, op2)[0];
                else
                    throw new LuaException("Invalid op");
            }
        }

        internal static LuaObject eq_event(LuaObject op1, LuaObject op2)
        {
            if (op1 == op2)
                return true;
            var handler = getEqualhandler(op1, op2);
            if (!handler.IsNil)
                return !(!(handler.Call(op1, op2)[0].AsBool()));
            else
                return false;
        }

        internal static LuaObject lt_event(LuaObject op1, LuaObject op2)
        {
            if (op1.IsNumber && op2.IsNumber)
                return op1.AsNumber() < op2.AsNumber();
            else if (op1.IsString && op2.IsString)
            {
                int n = StringComparer.CurrentCulture.Compare(op1.AsString(), op2.AsString());
                return (n < 0);
            }
            else
            {
                var handler = getBinhandler(op1, op2, "__lt");
                if (!handler.IsNil)
                    return !(!(handler.Call(op1, op2)[0].AsBool()));
                else
                    throw new ArgumentException("attempt to compare " + op1.type.ToString() + " with " + op2.type.ToString());
            }
        }

        internal static LuaObject le_event(LuaObject op1, LuaObject op2)
        {
            if (op1.IsNumber && op2.IsNumber)
                return op1.AsNumber() <= op2.AsNumber();
            else if (op1.IsString && op2.IsString)
            {
                int n = StringComparer.CurrentCulture.Compare(op1.AsString(), op2.AsString());
                return (n <= 0);
            }
            else
            {
                var handler = getBinhandler(op1, op2, "__le");
                if (!handler.IsNil)
                    return !(!(handler.Call(op1, op2)[0].AsBool()));
                else
                    throw new ArgumentException("attempt to compare " + op1.type.ToString() + " with " + op2.type.ToString());
            }
        }

        internal static LuaObject gc_event(LuaObject op)
        {
            var handler = getMetamethod(op, "__gc");
            if (handler.IsFunction)
                handler.AsFunction()(new LuaObject[] {op});
            return LuaObject.Nil;
        }

        internal static LuaObject tostring_event(LuaObject op)
        {
            var handler = getMetamethod(op, "__tostring");
            if (!handler.IsNil)
                return handler.Call(op)[0];
            else
                return op.ToString();
        }
    }
}
