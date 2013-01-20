﻿#region License
//
// Command Line Library: OptionInfo.cs
//
// Author:
//   Giacomo Stelluti Scala (gsscoder@gmail.com)
//
// Copyright (C) 2005 - 2013 Giacomo Stelluti Scala
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion

namespace CommandLine.Internal
{
    partial class OptionInfo
    {
        public bool HasParameterLessCtor
        {
            get { return _hasParameterLessCtor; }
            set
            {
                _hasParameterLessCtor = value;
            }
        }

        public object GetValue(object target)
        {          
            return _property.GetValue(target, null);
        }

        public void CreateInstance(object target)
        {
            try
            {
                _property.SetValue(target, Activator.CreateInstance(_property.PropertyType), null);
            }
            catch (Exception e)
            {
                throw new CommandLineParserException("Instance defined for verb command could not be created.", e);
            }
        }

        public static OptionMap CreateMap(object target,
            IList<Pair<PropertyInfo, VerbOptionAttribute>> verbs, CommandLineParserSettings settings)
        {
            var map = new OptionMap(verbs.Count, settings);
            foreach (var verb in verbs)
            {
                var optionInfo = new OptionInfo(verb.Right, verb.Left)
                    {
                        HasParameterLessCtor = verb.Left.PropertyType.GetConstructor(Type.EmptyTypes) != null

                    };
                if (!optionInfo.HasParameterLessCtor && verb.Left.GetValue(target, null) == null)
                {
                    throw new CommandLineParserException(string.Format("Type {0} must have a parameterless constructor or" +
                        " be already initialized to be used as a verb command.", verb.Left.PropertyType));
                }
                map[verb.Right.UniqueName] = optionInfo;
            }
            map.RawOptions = target;
            return map;
        }

        private bool _hasParameterLessCtor;
    }
}