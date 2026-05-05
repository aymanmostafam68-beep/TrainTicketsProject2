using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TrainTicketsProject.Interfaces;

namespace TrainTicketsProject.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDB.ApplicationDataAccess db;
        private readonly DbSet<T> _dbset;

        public Repository(ApplicationDB.ApplicationDataAccess dataAccess)
        {
            db = dataAccess;
            _dbset = db.Set<T>();

        }

        public async Task Create(T entity)
        {

            await _dbset.AddAsync(entity);
        }
        public async Task update(T entity)
        {

            _dbset.Update(entity);
        }
        public async Task Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public async Task DeleteRange(IEnumerable<T> entity)
        {
            _dbset.RemoveRange(entity);
        }

        public async Task AddRange(IEnumerable<T> entity)
        {
            await _dbset.AddRangeAsync(entity);
        }

        public async Task<int> Comment()
        {
            try
            {
                return await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return 0;
            }

        }


        public async Task<List<T>> GetAll(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IQueryable<T>>? includeFunc = null, bool tracked = true)
        {
            IQueryable<T> data = _dbset;
            if (expression != null) data = data.Where(expression);
            if (includeFunc != null) data = includeFunc(data);
            if (!tracked) data = data.AsNoTracking();
            return await data.ToListAsync();
        }
        
        
        
        public async Task<T?> GetOne(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IQueryable<T>>? includeFunc = null, bool tracked = true)
        {
            IQueryable<T> query = _dbset;

            if (!tracked)
                query = query.AsNoTracking();

            if (includeFunc != null)
                query = includeFunc(query);

            return await query.FirstOrDefaultAsync(filter);
        }


        public void OnefileUpload(IFormFile file, string location, out string newFileName, bool replcae = false)
        {

            if (file != null && file.Length > 0)
            {
                var filename = Guid.NewGuid().ToString().Substring(0, 7) + Path.GetExtension(file.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", location, filename);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                newFileName = filename;
            }
            else
            {
                newFileName = "default.png";
            }


        }






        public void manyFilesUpload(IFormFile[] file, string location, out List<string> newFileNames, bool replcae = false)
        {
            newFileNames = new List<string>();

            if (file != null && file.Length > 0)
            {
                foreach (var f in file)
                {

                    var filename = Guid.NewGuid().ToString().Substring(0, length: 7) + Path.GetExtension(f.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", location, filename);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        f.CopyTo(stream);
                    }

                    newFileNames.Add(filename);
                }


            }
            else
            {
                newFileNames.Add("default.png");
            }


        }
    }
}
