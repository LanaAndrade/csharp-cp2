-- Criar o banco de dados
IF DB_ID('LojaDB') IS NOT NULL
    DROP DATABASE LojaDB;
GO

CREATE DATABASE LojaDB;
GO

USE LojaDB;
GO

-- Tabela de Categorias
CREATE TABLE Categorias (
    Id INT IDENTITY PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL
);

-- Tabela de Produtos
CREATE TABLE Produtos (
    Id INT IDENTITY PRIMARY KEY,
    Nome VARCHAR(150) NOT NULL,
    Preco DECIMAL(10,2) NOT NULL,
    Estoque INT NOT NULL,
    CategoriaId INT NOT NULL,
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE CASCADE
);

-- Tabela de Clientes
CREATE TABLE Clientes (
    Id INT IDENTITY PRIMARY KEY,
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL
);

-- Tabela de Pedidos
CREATE TABLE Pedidos (
    Id INT IDENTITY PRIMARY KEY,
    ClienteId INT NOT NULL,
    DataPedido DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id) ON DELETE CASCADE
);

-- Tabela de Itens do Pedido
CREATE TABLE PedidoItens (
    Id INT IDENTITY PRIMARY KEY,
    PedidoId INT NOT NULL,
    ProdutoId INT NOT NULL,
    Quantidade INT NOT NULL,
    PrecoUnitario DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id)
);

-- Inserir categorias iniciais
INSERT INTO Categorias (Nome) VALUES
('Bebidas'), 
('Higiene'), 
('Alimentos'),
('Limpeza');

-- Inserir produtos iniciais
INSERT INTO Produtos (Nome, Preco, Estoque, CategoriaId) VALUES
('Coca-Cola Lata', 5.50, 50, 1),
('Suco de Laranja', 6.00, 30, 1),
('Arroz 5kg', 25.90, 20, 3),
('Feijão 1kg', 8.50, 40, 3),
('Sabonete Dove', 4.20, 100, 2),
('Detergente Ypê', 3.00, 60, 4);

-- Inserir clientes iniciais
INSERT INTO Clientes (Nome, Email) VALUES
('Lana Andrade', 'lana@example.com'),
('Pedro Silva', 'pedro@example.com'),
('Maria Souza', 'maria@example.com');
