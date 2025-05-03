using APIWithAuth.Entities;
using APIWithAuth.Model.Dto.Comment;
using APIWithAuth.Model.Dto.Tweet;
using APIWithAuth.Model.Dto.User;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace APIWithAuth;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TweetAddDto, TweetDto>().ReverseMap();
        CreateMap<TweetAddDto, Tweet>().ReverseMap();
        CreateMap<ApplicationUser, ApplicationUserDto>();
        CreateMap<CommentDto, Comment>().ReverseMap();
        CreateMap<CommentAddDto, Comment>();
        CreateMap<Tweet, CommentDto>();
        CreateMap<Tweet, TweetDto>();
    }
    
}