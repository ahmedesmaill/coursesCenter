﻿using coursesCenter.Models.data;
using coursesCenter.Models.entities;
using coursesCenter.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace coursesCenter.Repository
{
    public class TraineRepository : ITraineRepository
    {
        public ApplicationDbContext Context = new ApplicationDbContext();

        public void Delete(int id)
        {
            var traineDB = Context.Traines.Include(x=>x.ApplicationUser).FirstOrDefault(x=>x.Id==id);
            var user =Context.Users.FirstOrDefault(x=>x.Id==traineDB.ApplicationUser.Id);
            Context.Traines.Remove(traineDB);
            Context.Users.Remove(user);
            Context.SaveChanges();
        }

        public void Edit(Traine traine)
        {
            var traineDB = GetById(traine.Id);
            traineDB.Address = traine.Address;
            traineDB.DepartmentId = traine.DepartmentId;
            traineDB.Level = traine.Level;
            traineDB.Name = traine.Name;
            Context.SaveChanges();
            return;
        }

        public List<TraineDepartmentViewModel> GetAllInTraineDepartmentViewModel()
        {
            var traines = new List<TraineDepartmentViewModel>();
            var traine = Context.Traines.Include(tran => tran.Departrment).Include(traine => traine.CourseResults).ThenInclude(courseResult => courseResult.Course).ToList();
            foreach (var train in traine)
            {
                TraineDepartmentViewModel tran = new TraineDepartmentViewModel();
                tran.Name = train.Name;
                tran.Id = train.Id;
                tran.Department = train.Departrment!=null ? train.Departrment.Name : "none";
                foreach (var res in train.CourseResults)
                {
                    tran.Courses.Add(res.Course.Name ?? "none");
                }
                traines.Add(tran);
            }
            return traines;
        }
        public OneTraineResultViewModel AllResultOfOne(int id)
        {
            var traine = Context.Traines.Include(tran => tran.Departrment).Include(traine => traine.CourseResults).ThenInclude(courseResult => courseResult.Course).FirstOrDefault(x=>x.ApplicationUserId==id);
            var tran=new OneTraineResultViewModel();
            tran.Name = traine.Name;
            tran.Id = traine.Id;
            tran.Department = traine.Departrment != null ? traine.Departrment.Name : "none";
            foreach (var res in traine.CourseResults)
            {
                tran.Courses.Add(res.Course.Name ?? "none");
                tran.Result.Add((double)res.Degree);
            }
            return tran;
        }
        public Traine GetById(int id)
        {
            return Context.Traines.FirstOrDefault(t => t.Id == id);
        }

        public Traine GetByIdIncludeCouseResultIncludeCourse(int id)
        {
            return Context.Traines.Include(traine => traine.CourseResults).ThenInclude(result => result.Course).FirstOrDefault(traine => traine.Id == id);
        }

        public void Save(Traine traine)
        {
            Context.Traines.Add(traine);
            Context.SaveChanges();
            return;
        }

        public ShowStudentResultViewModel TraineShowStudentResultViewModel(int id)
        {
            var traineDB=GetByIdIncludeCouseResultIncludeCourse(id);
            if(traineDB == null)
            {
                return null;
            }
            var traine = new ShowStudentResultViewModel();
            traine.Name = traineDB.Name;
            foreach (var result in traineDB.CourseResults)
            {
                string col = result.Degree >= result.Course.MinDegree ? "Green" : "Red";
                Result res = new Result(result.Degree, result.Course.Name, col);
                traine.Results.Add(res);
            }
            return traine;
        }
        public List<Traine>GetTraineInCourseByCourseId(int courseId)
        {
            return Context.TraineCourses.Include(x=>x.Traine).Where(x=>x.CourseId==courseId).Select(x=>x.Traine).ToList();
        }
    }
}
