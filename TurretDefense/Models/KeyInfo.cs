using System.Collections.Specialized;
using System.Xml;
using Microsoft.Xna.Framework.Input;

namespace TurretDefense.Models;

public record KeyInfo(bool Rebindable, Keys? Key = null);

public record KeyInfoDto(bool Rebindable, int? Key = null);

public static class KeyInfoExt
{
    public static KeyInfoDto ToDto(this KeyInfo info) => new(info.Rebindable, (int?)info.Key);

    public static KeyInfo ToKeyInfo(this KeyInfoDto dto) => new(dto.Rebindable, (Keys?)dto.Key);
}
