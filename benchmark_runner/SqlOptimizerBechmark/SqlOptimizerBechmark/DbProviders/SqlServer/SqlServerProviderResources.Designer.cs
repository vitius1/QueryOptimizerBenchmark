﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlOptimizerBechmark.DbProviders.SqlServer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SqlServerProviderResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SqlServerProviderResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SqlOptimizerBechmark.DbProviders.SqlServer.SqlServerProviderResources", typeof(SqlServerProviderResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF NOT EXISTS(
        ///	SELECT *
        ///	FROM sys.schemas
        ///	WHERE name = &apos;sql_optimizer_benchmark&apos;
        ///) EXEC (&apos;CREATE SCHEMA [sql_optimizer_benchmark];&apos;);
        ///
        ///GO
        ///
        ///IF OBJECT_ID(&apos;sql_optimizer_benchmark.sp_GetQueryStatistics&apos;) IS NOT NULL
        ///DROP PROCEDURE [sql_optimizer_benchmark].[sp_GetQueryStatistics];
        ///
        ///GO
        ///
        ///-- =============================================
        ///-- Author:		Petr Lukas
        ///-- Create date: 2018-10-05
        ///-- Description:	Gets the query plan and query
        ///--              processing processing time.
        ///-- ================ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlInitScript {
            get {
                return ResourceManager.GetString("SqlInitScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BEGIN
        ///	DECLARE @testResultId INT;
        ///	DECLARE @sql NVARCHAR(MAX);
        ///	DECLARE @resultSize INT;
        ///	DECLARE @processingTime FLOAT;
        ///	DECLARE @queryPlan NVARCHAR(MAX);
        ///
        ///	INSERT INTO sql_optimizer_benchmark.test_results(test_group, configuration, test)
        ///	VALUES (&apos;{0}&apos;, &apos;{1}&apos;, &apos;{2}&apos;);
        ///	
        ///	SELECT @testResultId = IDENT_CURRENT(&apos;sql_optimizer_benchmark.test_results&apos;);
        ///
        ///	{3}
        ///END;.
        /// </summary>
        internal static string SqlTestScript {
            get {
                return ResourceManager.GetString("SqlTestScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 	SET @sql = &apos;{0}&apos;;
        ///	EXEC sql_optimizer_benchmark.sp_GetQueryStatistics @sql, @processingTime OUT, @queryPlan OUT;
        ///	INSERT INTO sql_optimizer_benchmark.variant_results(test_result_id, result_size, query_processing_time, query_plan)
        ///	VALUES (@testResultId, @resultSize, @processingTime, @queryPlan);
        ///.
        /// </summary>
        internal static string SqlVariantScript {
            get {
                return ResourceManager.GetString("SqlVariantScript", resourceCulture);
            }
        }
    }
}
