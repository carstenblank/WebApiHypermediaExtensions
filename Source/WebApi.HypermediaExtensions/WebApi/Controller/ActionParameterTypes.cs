﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebApi.HypermediaExtensions.ErrorHandling;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;
using WebApi.HypermediaExtensions.WebApi.Formatter;

namespace WebApi.HypermediaExtensions.WebApi.Controller
{
    public class ActionParameterSchemas
    {
        readonly ImmutableDictionary<string, object> schemaByTypeName;

        public ActionParameterSchemas(IEnumerable<Type> actionParameterTypes, bool lowercaseUrls)
        {
            schemaByTypeName = actionParameterTypes.ToImmutableDictionary(
                t => lowercaseUrls ? t.BeautifulName().ToLower() : t.BeautifulName(),
                t => JsonSchemaFactory.Generate(t).GetAwaiter().GetResult()
            );
        }

        public bool TryGetValue(string parameterTypeName, out object schema)
        {
            return schemaByTypeName.TryGetValue(parameterTypeName, out schema);
        }
    }

    [Route("ActionParmameterTypes")]
    public class ActionParameterTypes : Microsoft.AspNetCore.Mvc.Controller
    {
        readonly ActionParameterSchemas schemaByTypeName;

        public ActionParameterTypes(ActionParameterSchemas schemaByTypeName)
        {
            this.schemaByTypeName = schemaByTypeName;
        }


        [HttpGet("{parameterTypeName}", Name = RouteNames.ActionParameterTypes)]
        public ActionResult GetActionParameterTypeSchema(string parameterTypeName)
        {
            if (!schemaByTypeName.TryGetValue(parameterTypeName, out var schema))
            {
                return this.Problem(new ProblemJson
                {
                    ProblemType = $"Unknwon parameter type name: '{parameterTypeName}'",
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Title = "Unkown action parameter type"
                });
            }

            return Ok(schema);
        }
    }
}
