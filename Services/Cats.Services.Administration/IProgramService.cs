﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Cats.Models;

namespace Cats.Services.Administration
{
   public interface IProgramService:IDisposable
    {
        bool AddProgram(Program program);
        bool DeleteProgram(Program program);
        bool DeleteById(int id);
        bool EditProgram(Program program);
        Program FindById(int id);
        List<Program> GetAllProgram();
        List<Program> FindBy(Expression<Func<Program, bool>> predicate);
    }
}
