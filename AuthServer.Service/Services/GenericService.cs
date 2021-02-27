using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class GenericService<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<TEntity> _genericRepository;

        #region ctor
        public GenericService(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepository)
        {
            _unitOfWork = unitOfWork;
            _genericRepository = genericRepository;
        }
        #endregion

        public async Task<Response<TDto>> AddAsync(TDto entity)
        {
            //gelen TDto entity nesnesini Tentity e mapliyoruz
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);

            await _genericRepository.AddAsync(newEntity);//Eklemesini yaptık
            await _unitOfWork.CommitAsync();//Veri Tabanına yansıttık.

            var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
            return Response<TDto>.Success(newDto, 200);// methoda newDto nesnesini ve success kodunu ekliyruz.
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            //Data burada direk memory' e dolmuştur ve veri çekilmiştir.
            var products = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepository.GetAllAsync());
            return Response<IEnumerable<TDto>>.Success(products, 200);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var product = await _genericRepository.GetByIdAsync(id);

            if (product == null)
            {
                return Response<TDto>.Fail("ID bulunamadı", 404, true);
            }
            //Method dönüşte TDto istediği için önce Object.Mapper ile TDto ya çevirdik.
            //200 status kodu standartlara göre Response Body' sinde data olacak
            return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(product), 200);
        }

        public async Task<Response<NoDataDto>> Remove(int id)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(id);

            if (isExistEntity == null)
            {
                //Herhangi bir data dönmiceğimiz için NoDataDto sınıfını oluşturmuştuk.
                return Response<NoDataDto>.Fail("Id bulunamadı", 404, true);

            }
            _genericRepository.Remove(isExistEntity);
            await _unitOfWork.CommitAsync();

            //204 status kodu standartlara göre Response Body' de hiç bir şey olmayacak anlamına gelir
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<NoDataDto>> Update(TDto entity, int id)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity == null)
            {
                return Response<NoDataDto>.Fail("ID bulunamadı", 404, true);
            }
            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            _genericRepository.Update(updateEntity);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            //Data burada çekilmiştir ancak henüz memory' e veya db ye yansımamıştır.
            // Where sorgularında tüm listeyi memory'e atıp işlem yapmak ağır yüktür.
            //O yüzden memory'e almadan önce where sorguları yapılan ham data memory'e alınır.
            var list = _genericRepository.Where(predicate);

            return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()),200);
        }
    }
}
