using AutoMapper;
using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.Profiles;

// Entity -> response DTO only. Request DTOs are bound straight by the controllers
// (a request has fewer fields than the entity it seeds, so hand-mapping there is clearer).
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemDto>();
        CreateMap<User, UserDto>();

        // Item/Borrower must be loaded (Include) by the caller; the flattened names have no source match.
        CreateMap<Borrowing, BorrowingDto>()
            .ForCtorParam(nameof(BorrowingDto.ItemName), o => o.MapFrom(b => b.Item.Name))
            .ForCtorParam(nameof(BorrowingDto.BorrowerName),
                o => o.MapFrom(b => b.Borrower.FirstName + " " + b.Borrower.LastName));
    }
}
