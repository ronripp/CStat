﻿using Dropbox.Api.TeamLog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;

namespace CStat.Pages.Tasks
{
    public class AutoGen
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;


        public AutoGen(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            hostEnv = hstEnv;
        }

        public List<CTask> GenTasks ()
        {
            // Get a List of Template Tasks
            var TemplateTasks = _context.Task.Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToList();

            // Determine what needs to be Auto generated
            foreach (var tmpl in TemplateTasks)
            {
                var AutoGenList = _context.Task.Include(t => t.Person).Where(t => (t.ParentTaskId == tmpl.Id)).ToList();

            }

            // Filter out existing Auto generated Tasks having a parent task id that matches template task id and due date.
            foreach (var tmpl in TemplateTasks)
            {
                var AutoGenList = _context.Task.Include(t => t.Person).Where(t => (t.ParentTaskId == tmpl.Id)).ToList();

            }


            // Generate active tasks 

            return null;
        }

    }
}