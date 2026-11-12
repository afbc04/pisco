# Anotações e Lembretes

## Tarefas e Planos

Aqui é onde são feitas as anotações com coisas que devem ser feitas e planeiam ser feitas.
Estrutura:

```json
{
    "id" : 1,
    "title" : "Tarefa",
    "description" : "Descrição da tarefa",
    "creation_date" : "2026-09-22",
    "finish_date" : "2026-09-23",
    "scheduled_finish_date" : "2026-09-31",
    "notes" : 
    [
        {
            "note" : "Nota 1",
            "date" : "2026-09-23"
        }
    ],
    "status" : "completed",
    "tasks" :
    [
        {
            "task" : "fazer coisa 1",
            "status" : "done",
            "status_message" : "foi tratado internamente pelo pessoal docente"
        }
    ]
}
```

Planos são conjunto de tarefas

## Lembretes

Indica mini tarefas que podem ser anotadas durante o dia

Se o lembrete não for starred e passar mais de 5 dias, ele é eliminado automaticamente

Se a mensagem começar com !, ele automaticamente é starred

```json
{
    "id" : 1,
    "note" : "lavar a roupa",
    "date" : "2026-09-23",
    "completed" : false,
    "starred" : false
}
```

## Eventos

Indica eventos futuros, podendo ser periódicos ou não

```json
{
    "id" : 1,
    "title" : "Aniversário Shami",
    "description" : "auto-explicativo",
    "creation_date" : "2026-09-22",
    "date" : "2026-06-07",
    "periodic" : "yearly",
    "type" : "aniversary",
    "deleted" : false,
    "active" : true
}
```

# Receitas e Despesas

## Carteiras

Indica eventos futuros, podendo ser periódicos ou não

```json
{
    "id" : 1,
    "name" : "Cartão de debito",
    "description" : "auto-explicativo",
    "creation_date" : "2026-09-22",
    "date" : "2026-06-07",
    "periodic" : "yearly",
    "type" : "aniversary",
    "deleted" : false,
    "active" : true
}
```

## Transações

## Categorias

## Pagamentos Pendentes

# Tags

# Parâmetros

## Estados

```json
[
    "draft",
    "not_started",
    "pending",
    "stalled",
    "deleted",
    "completed",
    "cancelled"
]
```