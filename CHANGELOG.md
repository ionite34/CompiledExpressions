# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2024-08-03
### Changed
- `CompiledInstanceAccessor` now inherits `CompiledAccessor` and are now classes instead of structs.
- Performance improvements for internal operations.
### Fixed
- Fixed `CompiledExpression.CreateAccessor(Expression<Func<T, TValue>> getExpression, Expression<Action<T, TValue>> setExpression)` overload not valid as an expression tree. Second argument is now `Action<T, TValue> setAction`.

## [1.0.0] - 2024-07-29
### Added
- Initial release.

## [1.0.0-pre.1] - 2024-07-28
### Added
- Initial release.
