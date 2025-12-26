# 🚀 A1Click - Sistema de Gestión de Inventario Resiliente (.NET + SQL Server)

> **Sistema de Punto de Venta diseñado con arquitectura offline-first, integridad ACID y automatización de infraestructura.**

[![Ver Demo en Video](https://img.youtube.com/vi/PONER_AQUI_ID_VIDEO/0.jpg)](PONER_AQUI_LINK_YOUTUBE)
*(Clic en la imagen para ver la demo de 1 min)*

---

## 📸 Galería Rápida

| Venta Rápida (Teclado) | Alerta de Stock Automática |
| :---: | :---: |
| ![Venta Rápida](img/Venta_Module.gif) | ![Alerta Stock](img/Alerta_Stock.gif) |

---

## 🏗️ Arquitectura de Solución
El sistema prioriza la integridad de datos y la trazabilidad forense, implementando un mecanismo de "Defensa en Profundidad" a nivel de base de datos.

### Mecanismo de Seguridad y Auditoría (ACID)
```mermaid
flowchart TD
    %% Estilos para diferenciar capas
    classDef actor fill:#eceff1,stroke:#37474f,stroke-width:2px;
    classDef sp fill:#e3f2fd,stroke:#1565c0,stroke-width:2px;
    classDef table fill:#fff3e0,stroke:#e65100,stroke-width:2px;
    classDef trigger fill:#fce4ec,stroke:#880e4f,stroke-width:2px,stroke-dasharray: 5 5;
    classDef ledger fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px;

    User([Usuario / App]):::actor -->|Intento de Cambio| SP(Stored Procedure<br/>Transaccional):::sp

    subgraph SAFETY [Mecanismo de Seguridad ACID]
        direction TB
        SP -->|1. UPDATE Stock| Prod[(Tabla: Productos)]:::table
        
        Prod -.->|Dispara| Trg(Trigger: trg_productos_touch):::trigger
        Trg -->|2. Actualiza Timestamp| Prod
        
        SP -->|3. INSERT Auditoría| Kardex[(Tabla: Movimientos<br/>Libro Mayor Inmutable)]:::ledger
    end

    Kardex -->|Registro Forense| Data1[Quién: Leonardo<br/>Qué: Venta<br/>Cuándo: 10:05:01]
    Prod -->|Sincronización| Data2[Última Modificación:<br/>2025-10-27 10:05:01]

    %% Nota de integridad
    linkStyle 2,4 stroke:#2e7d32,stroke-width:3px;
```

## 💻 Ingeniería de Software (Snippets Destacados)

> *Nota: Este es un proyecto de código cerrado (Proprietary Software). Se presentan fragmentos clave para demostrar la calidad de la arquitectura.*

### 🔐 1. Integridad de Datos (Backend SQL)
Implementación de transacciones atómicas para asegurar que el inventario y la caja siempre cuadren.
* 📄 **Ver Código:** [ACID_Transaction_Snippet.sql](src/database_snippets/ACID_Transaction_Snippet.sql)
* 📄 **Ver Código:** [Async_Outbox_Pattern_Snippet.sql](src/database_snippets/Async_Outbox_Pattern_Snippet.sql)

### 🛡️ 2. Resiliencia y UX (Frontend C#)
Manejo de fallos de red y optimización de flujos de trabajo mediante atajos de teclado globales.
* 📄 **Ver Código:** [Resiliencia_json_Snippet.cs](src/csharp_snippets/Resiliencia_json_Snippet.cs) *(Persistencia local ante fallos)*
* 📄 **Ver Código:** [UX_AtajosTeclado_Snippet.cs](src/csharp_snippets/UX_AtajosTeclado_Snippet.cs) *(Interceptación de teclas a bajo nivel)*

### 🧠 3. Lógica de Dominio (Core C#)
Encapsulamiento de reglas de negocio y validación de cuadratura financiera previo a la persistencia.
* 📄 **Ver Código:** [VentaService_LogicaNegocio.cs](src/csharp_snippets/VentaService_LogicaNegocio.cs) *(Validaciones de integridad y reglas de negocio)*

---

## 📄 Documentación Completa
Para un análisis profundo de las decisiones de ingeniería, consulte los informes técnicos:

* 📘 **[Informe de Arquitectura de Aplicación (PDF)](Informe_App_A1Click.pdf)**
* 📙 **[Informe de Ingeniería de Datos (PDF)](Informe_baseDatos_A1Click.pdf)**

---
**Desarrollado por Leonardo Ahumada** | *Ingeniero de Software .NET / SQL*