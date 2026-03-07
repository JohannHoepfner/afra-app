import { GraphQLClient } from 'graphql-request';

/**
 * A shared GraphQL client that sends requests to the /graphql endpoint.
 * Authentication is handled automatically via session cookies.
 */
export const graphqlClient = new GraphQLClient('/graphql', {
    credentials: 'include',
});

/**
 * Executes a GraphQL query and returns the result data.
 *
 * @param query - The GraphQL query string
 * @param variables - Optional query variables
 * @returns The query result data
 */
export async function gql<T = Record<string, unknown>>(
    query: string,
    variables?: Record<string, unknown>,
): Promise<T> {
    return graphqlClient.request<T>(query, variables);
}
