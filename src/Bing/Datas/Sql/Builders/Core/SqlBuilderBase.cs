﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Bing.Datas.Matedatas;
using Bing.Datas.Sql.Builders.Clauses;
using Bing.Datas.Sql.Builders.Filters;
using Bing.Domains.Repositories;

namespace Bing.Datas.Sql.Builders.Core
{
    /// <summary>
    /// Sql生成器基类
    /// </summary>
    public abstract class SqlBuilderBase:ISqlBuilder, IClauseAccessor
    {
        #region 字段

        /// <summary>
        /// 参数管理器
        /// </summary>
        private IParameterManager _parameterManager;

        /// <summary>
        /// 方言
        /// </summary>
        private IDialect _dialect;

        /// <summary>
        /// Select子句
        /// </summary>
        private ISelectClause _selectClause;

        /// <summary>
        /// From子句
        /// </summary>
        private IFromClause _fromClause;

        /// <summary>
        /// Join子句
        /// </summary>
        private IJoinClause _joinClause;

        /// <summary>
        /// Where子句
        /// </summary>
        private IWhereClause _whereClause;

        /// <summary>
        /// 分组字句
        /// </summary>
        private IGroupByClause _groupByClause;

        /// <summary>
        /// 排序子句
        /// </summary>
        private IOrderByClause _orderByClause;

        /// <summary>
        /// 参数字面值解析器
        /// </summary>
        private IParamLiteralsResolver _paramLiteralsResolver;

        /// <summary>
        /// 是否已添加过滤器
        /// </summary>
        private bool _isAddFilters;

        #endregion

        #region 属性

        /// <summary>
        /// 实体元数据解析器
        /// </summary>
        protected IEntityMatedata EntityMatedata { get; private set; }

        /// <summary>
        /// 实体解析器
        /// </summary>
        protected IEntityResolver EntityResolver { get; private set; }

        /// <summary>
        /// 实体别名注册器
        /// </summary>
        protected IEntityAliasRegister AliasRegister { get; private set; }

        /// <summary>
        /// 参数管理器
        /// </summary>
        protected IParameterManager ParameterManager => _parameterManager ?? (_parameterManager = CreateParameterManager());

        /// <summary>
        /// Sql方言
        /// </summary>
        protected IDialect Dialect => _dialect ?? (_dialect = GetDialect());

        /// <summary>
        /// Select子句
        /// </summary>
        public ISelectClause SelectClause => _selectClause ?? (_selectClause = CreateSelectClause());

        /// <summary>
        /// From子句
        /// </summary>
        public IFromClause FromClause => _fromClause ?? (_fromClause = CreateFromClause());

        /// <summary>
        /// Join子句
        /// </summary>
        public IJoinClause JoinClause => _joinClause ?? (_joinClause = CreateJoinClause());

        /// <summary>
        /// Where子句
        /// </summary>
        public IWhereClause WhereClause => _whereClause ?? (_whereClause = CreateWhereClause());

        /// <summary>
        /// 分组子句
        /// </summary>
        public IGroupByClause GroupByClause => _groupByClause ?? (_groupByClause = CreateGroupByClause());

        /// <summary>
        /// 排序子句
        /// </summary>
        public IOrderByClause OrderByClause => _orderByClause ?? (_orderByClause = CreateOrderByClause());

        /// <summary>
        /// 参数字面值解析器
        /// </summary>
        protected IParamLiteralsResolver ParamLiteralsResolver => _paramLiteralsResolver ?? (_paramLiteralsResolver = GetParamLiteralsResolver());

        /// <summary>
        /// 跳过行数参数名
        /// </summary>
        protected string OffsetParam { get; private set; }

        /// <summary>
        /// 限制行数参数名
        /// </summary>
        protected string LimitParam { get; private set; }

        /// <summary>
        /// 分页
        /// </summary>
        public IPager Pager { get; private set; }

        /// <summary>
        /// 是否分组
        /// </summary>
        public bool IsGroup => GroupByClause.IsGroup;

        /// <summary>
        /// 是否限制行数
        /// </summary>
        protected bool IsLimit => string.IsNullOrWhiteSpace(LimitParam) == false;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化一个<see cref="SqlBuilderBase"/>类型的实例
        /// </summary>
        /// <param name="matedata">实体元数据解析器</param>
        /// <param name="parameterManager">参数管理器</param>
        protected SqlBuilderBase(IEntityMatedata matedata = null, IParameterManager parameterManager = null)
        {
            EntityMatedata = matedata;
            _parameterManager = parameterManager;
            EntityResolver = new EntityResolver(matedata);
            AliasRegister = new EntityAliasRegister();
            Pager = new Pager();
        }

        #endregion

        #region 工厂方法

        /// <summary>
        /// 创建参数管理器
        /// </summary>
        /// <returns></returns>
        protected virtual IParameterManager CreateParameterManager()
        {
            return new ParameterManager(Dialect);
        }

        /// <summary>
        /// 获取Sql方言
        /// </summary>
        /// <returns></returns>
        protected abstract IDialect GetDialect();

        /// <summary>
        /// 创建Select子句
        /// </summary>
        /// <returns></returns>
        protected virtual ISelectClause CreateSelectClause()
        {
            return new SelectClause(this, Dialect, EntityResolver, AliasRegister);
        }

        /// <summary>
        /// 创建From子句
        /// </summary>
        /// <returns></returns>
        protected virtual IFromClause CreateFromClause()
        {
            return new FromClause(this, Dialect, EntityResolver, AliasRegister);
        }

        /// <summary>
        /// 创建Join子句
        /// </summary>
        /// <returns></returns>
        protected virtual IJoinClause CreateJoinClause()
        {
            return new JoinClause(this, Dialect, EntityResolver, AliasRegister);
        }

        /// <summary>
        /// 创建Where子句
        /// </summary>
        /// <returns></returns>
        protected virtual IWhereClause CreateWhereClause()
        {
            return new WhereClause(this, Dialect, EntityResolver, AliasRegister, ParameterManager);
        }

        /// <summary>
        /// 创建分组子句
        /// </summary>
        /// <returns></returns>
        protected virtual IGroupByClause CreateGroupByClause()
        {
            return new GroupByClause(Dialect, EntityResolver, AliasRegister);
        }

        /// <summary>
        /// 创建排序子句
        /// </summary>
        /// <returns></returns>
        protected virtual IOrderByClause CreateOrderByClause()
        {
            return new OrderByClause(Dialect, EntityResolver, AliasRegister);
        }

        /// <summary>
        /// 获取参数字面值解析器
        /// </summary>
        /// <returns></returns>
        protected virtual IParamLiteralsResolver GetParamLiteralsResolver()
        {
            return new ParamLiteralsResolver();
        }

        #endregion

        #region Clone(克隆)

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public abstract ISqlBuilder Clone();

        /// <summary>
        /// 克隆
        /// </summary>
        /// <param name="sqlBuilder">源生成器</param>
        protected void Clone(SqlBuilderBase sqlBuilder)
        {
            EntityMatedata = sqlBuilder.EntityMatedata;
            _parameterManager = sqlBuilder._parameterManager?.Clone();
            EntityResolver = sqlBuilder.EntityResolver ?? new EntityResolver(EntityMatedata);
            AliasRegister = sqlBuilder.AliasRegister?.Clone() ?? new EntityAliasRegister();
            _selectClause = sqlBuilder._selectClause?.Clone(this, AliasRegister);
            _fromClause = sqlBuilder._fromClause?.Clone(this, AliasRegister);
            _joinClause = sqlBuilder._joinClause?.Clone(this, AliasRegister);
            _whereClause = sqlBuilder._whereClause?.Clone(this, AliasRegister, _parameterManager);
            _groupByClause = sqlBuilder._groupByClause?.Clone(AliasRegister);
            _orderByClause = sqlBuilder._orderByClause?.Clone(AliasRegister);
            Pager = sqlBuilder.Pager;
            OffsetParam = sqlBuilder.OffsetParam;
            LimitParam = sqlBuilder.LimitParam;
        }

        #endregion

        #region Clear(清空)

        /// <summary>
        /// 清空并初始化
        /// </summary>
        public void Clear()
        {
            AliasRegister = new EntityAliasRegister();
            ClearSelect();
            ClearFrom();
            ClearJoin();
            ClearWhere();
            ClearGroupBy();
            ClearOrderBy();
            ClearSqlParams();
            ClearPageParams();
        }

        /// <summary>
        /// 清空Select子句
        /// </summary>
        public void ClearSelect()
        {
            _selectClause = CreateSelectClause();
        }

        /// <summary>
        /// 清空From子句
        /// </summary>
        public void ClearFrom()
        {
            _fromClause = CreateFromClause();
        }

        /// <summary>
        /// 清空Join子句
        /// </summary>
        public void ClearJoin()
        {
            _joinClause = CreateJoinClause();
        }

        /// <summary>
        /// 清空Where子句
        /// </summary>
        public void ClearWhere()
        {
            _isAddFilters = false;
            _whereClause = CreateWhereClause();
        }

        /// <summary>
        /// 清空GroupBy子句
        /// </summary>
        public void ClearGroupBy()
        {
            _groupByClause = CreateGroupByClause();
        }

        /// <summary>
        /// 清空OrderBy子句
        /// </summary>
        public void ClearOrderBy()
        {
            _orderByClause = CreateOrderByClause();
        }

        /// <summary>
        /// 清空Sql参数
        /// </summary>
        public void ClearSqlParams()
        {
            _parameterManager.Clear();
        }

        /// <summary>
        /// 清空分页参数
        /// </summary>
        public void ClearPageParams()
        {
            Pager = null;
            OffsetParam = null;
            LimitParam = null;
        }

        #endregion

        #region New(创建Sql生成器)

        /// <summary>
        /// 创建Sql生成器
        /// </summary>
        /// <returns></returns>
        public abstract ISqlBuilder New();

        #endregion

        #region ToDebugSql(生成调试Sql语句)

        /// <summary>
        /// 生成调试Sql语句，Sql语句中的参数被替换为参数值
        /// </summary>
        /// <returns></returns>
        public virtual string ToDebugSql()
        {
            return GetDebugSql(ToSql());
        }

        /// <summary>
        /// 获取调试Sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string GetDebugSql(string sql)
        {
            var parameters = GetParams();
            foreach (var parameter in parameters)
            {
                sql = Regex.Replace(sql, $@"{parameter.Key}\b",
                    ParamLiteralsResolver.GetParamLiterals(parameter.Value));
            }

            return sql;
        }

        #endregion

        #region ToSql(生成Sql)

        /// <summary>
        /// 生成Sql语句
        /// </summary>
        /// <returns></returns>
        public virtual string ToSql()
        {
            Init();
            Validate();
            var result = new StringBuilder();
            CreateSql(result);
            return result.ToString().Trim();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            OrderByClause.OrderBy(Pager?.Order);
        }

        /// <summary>
        /// 验证
        /// </summary>
        public virtual void Validate()
        {
            FromClause.Validate();
            OrderByClause.Validate(IsLimit);
        }

        /// <summary>
        /// 创建Sql语句
        /// </summary>
        /// <param name="result"></param>
        protected virtual void CreateSql(StringBuilder result)
        {
            AppendSelect(result);
            AppendFrom(result);
            AppendSql(result, JoinClause.ToSql());
            AppendSql(result, GetWhere());
            AppendSql(result, GroupByClause.ToSql());
            AppendSql(result, OrderByClause.ToSql());
            AppendLimit(result);
        }

        /// <summary>
        /// 添加Sql
        /// </summary>
        /// <param name="result">Sql拼接</param>
        /// <param name="sql">Sql语句</param>
        protected void AppendSql(StringBuilder result, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            result.AppendLine($"{sql} ");
        }

        /// <summary>
        /// 添加Select子句
        /// </summary>
        /// <param name="result">Sql拼接器</param>
        protected virtual void AppendSelect(StringBuilder result)
        {
            var sql = SelectClause.ToSql();
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidOperationException("必须设置Select子句");
            }

            AppendSql(result, sql);
        }

        /// <summary>
        /// 添加From子句
        /// </summary>
        /// <param name="result">Sql拼接器</param>
        protected virtual void AppendFrom(StringBuilder result)
        {
            var sql = FromClause.ToSql();
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidOperationException("必须设置From子句");
            }

            AppendSql(result, sql);
        }

        /// <summary>
        /// 获取Where语句
        /// </summary>
        /// <returns></returns>
        public virtual string GetWhere()
        {
            if (_isAddFilters == false)
            {
                AddFilters();
            }

            return WhereClause.ToSql();
        }

        /// <summary>
        /// 添加过滤器列表
        /// </summary>
        private void AddFilters()
        {
            _isAddFilters = true;
            var context = new SqlContext(AliasRegister, WhereClause, EntityMatedata);
            SqlFilterCollection.Filters.ForEach(filter => filter.Filter(context));
        }

        /// <summary>
        /// 添加分页Sql
        /// </summary>
        /// <param name="result">Sql拼接器</param>
        private void AppendLimit(StringBuilder result)
        {
            if (IsLimit)
            {
                AppendSql(result, CreateLimitSql());
            }
        }

        /// <summary>
        /// 创建分页Sql
        /// </summary>
        protected abstract string CreateLimitSql();

        #endregion

        #region AddParam(添加参数)

        /// <summary>
        /// 添加Sql参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        public void AddParam(string name, object value)
        {
            ParameterManager.Add(name, value);
        }

        #endregion

        #region GetParams(获取参数)

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, object> GetParams()
        {
            return ParameterManager.GetParams();
        }

        #endregion

        #region GetCondition(获取查询条件)

        /// <summary>
        /// 获取查询条件
        /// </summary>
        /// <returns></returns>
        public virtual string GetCondition()
        {
            return WhereClause.GetCondition();
        }

        #endregion

        #region Pager(设置分页)

        /// <summary>
        /// 设置跳过行数
        /// </summary>
        /// <param name="count">跳过的行数</param>
        /// <returns></returns>
        public ISqlBuilder Skip(int count)
        {
            var param = GetOffsetParam();
            ParameterManager.Add(param, count);
            return this;
        }

        /// <summary>
        /// 获取跳过行数的参数名
        /// </summary>
        /// <returns></returns>
        protected string GetOffsetParam()
        {
            if (string.IsNullOrWhiteSpace(OffsetParam) == false)
            {
                return OffsetParam;
            }

            OffsetParam = ParameterManager.GenerateName();
            ParameterManager.Add(OffsetParam, 0);
            return OffsetParam;
        }

        /// <summary>
        /// 设置获取行数
        /// </summary>
        /// <param name="count">获取的行数</param>
        /// <returns></returns>
        public ISqlBuilder Take(int count)
        {
            var param = GetLimitParam();
            ParameterManager.Add(param, count);
            Pager.PageSize = count;
            return this;
        }

        /// <summary>
        /// 获取限制行数的参数名
        /// </summary>
        /// <returns></returns>
        protected string GetLimitParam()
        {
            if (string.IsNullOrWhiteSpace(LimitParam) == false)
            {
                return LimitParam;
            }

            LimitParam = ParameterManager.GenerateName();
            return LimitParam;
        }

        /// <summary>
        /// 设置分页
        /// </summary>
        /// <param name="pager">分页参数</param>
        /// <returns></returns>
        public ISqlBuilder Page(IPager pager)
        {
            if (pager == null)
            {
                return this;
            }

            Pager = pager;
            Skip(pager.GetSkipCount()).Take(pager.PageSize);
            return this;
        }

        #endregion
    }
}
