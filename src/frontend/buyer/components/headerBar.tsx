import React from 'react'
import { Box, Flex, Spacer, Text } from "@chakra-ui/react"
import Link from 'next/link'

export const HeaderBar = () => {
    return (
        <Flex as="header" 
            position="fixed" 
            w="100%" 
            h="100px" 
            justifyContent={'center'}
            backgroundColor="green.600" 
            style={{transition: '0.2s', zIndex: 1000}}
        >
            <Flex direction={'row'} w={1200} h="100%" alignItems={'center'}>
                <Box>

                </Box>
                <Spacer />
                <Box>
                    <Link href="/">
                        <Text color='white' textAlign={'center'} fontSize='5xl' fontWeight={900} cursor='pointer'>Summer Olympic Games 2024</Text>
                    </Link>
                </Box>
                <Spacer />
                <Box>

                </Box>
            </Flex>
        </Flex>
    )
}
